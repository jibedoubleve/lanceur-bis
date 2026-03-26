using System.Text.RegularExpressions;
using Humanizer;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public sealed class SearchServiceOrchestrator : ISearchServiceOrchestrator
{
    #region Fields

    private readonly ISection<StoreSection> _settings;

    #endregion

    #region Constructors

    public SearchServiceOrchestrator(ISection<StoreSection> settings) => _settings = settings;

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool IsAlive(IStoreService storeService, Cmdline query)
        => new Regex(
            storeService.StoreOrchestration.AlivePattern,
            RegexOptions.Compiled,
            200.Milliseconds()
        ).IsMatch(query.Name);

    #endregion
}