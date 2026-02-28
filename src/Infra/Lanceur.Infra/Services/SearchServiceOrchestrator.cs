using System.Text.RegularExpressions;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public class SearchServiceOrchestrator : ISearchServiceOrchestrator
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
    {
        if (storeService is null) { return false; }

        _settings.Reload();
        var storeOverride = _settings.Value.StoreShortcuts
                                     .FirstOrDefault(x => x.StoreType == storeService.GetType().ToString());

        var regex = new Regex(storeOverride?.AliasOverride ?? storeService.StoreOrchestration.AlivePattern);

        var isAlive = regex.IsMatch(query.Name);
        return isAlive;
    }

    #endregion
}