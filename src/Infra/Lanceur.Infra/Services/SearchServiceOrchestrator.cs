using System.Text.RegularExpressions;
using Lanceur.Core.Configuration;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class SearchServiceOrchestrator : ISearchServiceOrchestrator
{
    #region Fields

    private readonly ISection<StoreSection> _settings;

    #endregion

    #region Constructors

    public SearchServiceOrchestrator(ILoggerFactory factory, ISection<StoreSection> settings)
    {
        _settings = settings;
        factory.GetLogger<SearchServiceOrchestrator>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool IsAlive(IStoreService storeService, Cmdline query)
    {
        if (storeService is null) return false;

        var storeOverride = _settings.Value.StoreShortcuts
                                     .FirstOrDefault(x => x.StoreType == storeService.GetType().ToString());
        var regex = new Regex(storeOverride?.AliasOverride ?? storeService.StoreOrchestration.AlivePattern);

        var isAlive = regex.IsMatch(query.Name);
        return isAlive;
    }

    #endregion
}