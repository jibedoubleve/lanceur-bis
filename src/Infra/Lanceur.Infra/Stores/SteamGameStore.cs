using System.Text.RegularExpressions;
using Humanizer;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Stores;

[Store(@"^\s{0,}&(.*)")]
public sealed class SteamGameStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ISteamLibraryService _steamLibraryService;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public SteamGameStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ISection<StoreSection> storeSettings,
        ISteamLibraryService steamLibraryService,
        IAliasManagementService aliasManagementService,
        IFeatureFlagService featureFlagService) : base(orchestrationFactory, storeSettings)
    {
        ArgumentNullException.ThrowIfNull(steamLibraryService);
        ArgumentNullException.ThrowIfNull(aliasManagementService);
        ArgumentNullException.ThrowIfNull(featureFlagService);

        _steamLibraryService = steamLibraryService;
        _aliasManagementService = aliasManagementService;
        _featureFlagService = featureFlagService;

        ShortcutRegex = new Lazy<Regex>(() => new Regex(Shortcut, RegexOptions.Compiled, 250.Milliseconds()));
    }

    #endregion

    #region Properties

    /// <inheritdoc cref="IStoreService.IsOverridable" />
    public override bool IsOverridable => true;

    public Lazy<Regex> ShortcutRegex { get; }

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration
        => _featureFlagService.IsEnabled(Features.SteamIntegration)
            ? StoreOrchestrationFactory.Exclusive(Shortcut)
            : StoreOrchestrationFactory.AlwaysInactive();

    #endregion

    #region Methods

    private static bool IsRefinementOf(SteamGame candidate, string searchKey)
        => IsRefinementOf(candidate.Name, searchKey);

    private static bool IsRefinementOf(QueryResult candidate, string searchKey)
        => IsRefinementOf(candidate.Name, searchKey);

    private static bool IsRefinementOf(string candidate, string searchKey)
        => candidate.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase);

    private bool IsUnfiltered(string previous)
    {
        var splits = ShortcutRegex.Value.Match(previous);

        if (!splits.Success) { return false; }

        return splits.Groups[1].Value.Length == 0;
    }

    private static string PropertySelector(Cmdline query) => query.Parameters;

    /// <inheritdoc cref="CanPruneResult" />
    public override bool CanPruneResult(Cmdline previous, Cmdline current)
        => OverrideCanPruneResult(
            previous,
            current,
            candidate => candidate.Parameters,
            (p, c) => IsRefinementOf(c, p) && !IsUnfiltered(p)
        );

    /// <inheritdoc cref="PruneResult" />
    public override int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current)
        => OverridePruneResult(
            destination,
            previous,
            current,
            PropertySelector,
            IsRefinementOf
        );

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var games = _steamLibraryService.GetGames();
        var results = games.Where(i => IsRefinementOf(i, PropertySelector(cmdline)))
                           .Select(x => new AliasQueryResult
                           {
                               Name = x.Name,
                               Description = "Steam library game",
                               FileName = x.ToSteamUrl()
                           }).ToList();
        results = _aliasManagementService.HydrateSteamGameUsage(results).ToList();
        var res = results.OrderByDescending(x => x.Count)
                         .ThenBy(x => x.Name);
        return res;
    }

    #endregion
}