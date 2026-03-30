using System.Text.RegularExpressions;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store(":")]
public sealed class AdditionalParametersStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasService;
    private readonly IFeatureFlagService _featureFlags;
    private readonly ILogger<AdditionalParametersStore> _logger;

    #endregion

    #region Constructors

    public AdditionalParametersStore(
        IStoreOrchestrationFactory orchestrationFactory,
        IAliasRepository aliasService,
        ILogger<AdditionalParametersStore> logger,
        IFeatureFlagService featureFlags,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings)
    {
        _aliasService = aliasService;
        _logger = logger;
        _featureFlags = featureFlags;

        ShortcutRegex =
            new Lazy<Regex>(() =>
                new Regex(ShortcutRegexString, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200)));
    }

    #endregion

    #region Properties

    private Lazy<Regex> ShortcutRegex { get; }

    private string ShortcutRegexString => $"(.*){Shortcut}(.*)";

    /// <inheritdoc cref="IStoreService.IsOverridable" />
    public override bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration
        => _featureFlags.IsEnabled(Features.AdditionalParameterAlwaysActive)
            ? StoreOrchestrationFactory.SharedAlwaysActive()
            : StoreOrchestrationFactory.Shared(ShortcutRegexString);

    #endregion

    #region Methods

    private bool IsRefinementOf(string value, string check)
        => !IsUnfiltered(check)
           && check.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);

    private bool IsRefinementOf(QueryResult query, string value)
        => IsRefinementOf(value, query.Name);


    private bool IsUnfiltered(Cmdline previous)
    {
        var splits = ShortcutRegex.Value.Match(previous);

        if (!splits.Success) { return false; }

        return splits.Groups[2].Value.Length == 0;
    }

    private static string SelectProperty(Cmdline cmdline) => cmdline.Name;

    /// <inheritdoc cref="CanPruneResult" />
    public override bool CanPruneResult(Cmdline previous, Cmdline current)
    {
        if (SelectProperty(current).Length == 0) { return false; }

        return OverrideCanPruneResult(
            previous,
            current,
            SelectProperty,
            IsRefinementOf
        );
    }

    /// <inheritdoc cref="IStoreService.GetAll" />
    public override IEnumerable<QueryResult> GetAll() => _aliasService.GetAllAliasWithAdditionalParameters();

    public override int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current)
        => OverridePruneResult(
            destination,
            previous,
            current,
            SelectProperty,
            IsRefinementOf
        );

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var _ = _logger.WarnIfSlow(this);
        return _aliasService.SearchAliasWithAdditionalParameters(cmdline.Name);
    }

    #endregion
}