using System.Text.RegularExpressions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class SearchServiceOrchestrator : ISearchServiceOrchestrator
{
    private readonly ISettingsFacade _settingsFacade;

    #region Fields

    private readonly ILogger<SearchServiceOrchestrator> _log;

    private readonly Dictionary<string, IStoreService> _stores = new();

    #endregion

    #region Constructors

    public SearchServiceOrchestrator(ILoggerFactory factory, ISettingsFacade settingsFacade)
    {
        _settingsFacade = settingsFacade;
        _log = factory.GetLogger<SearchServiceOrchestrator>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool IsAlive(IStoreService storeService, Cmdline query)
    {
        if (storeService is null) return false;

        var storeOverride = _settingsFacade.Application.Stores.StoreOverrides
                                           .FirstOrDefault(x => x.StoreType == storeService.GetType().ToString());
        var regex = new Regex(storeOverride?.AliasOverride ?? storeService.StoreOrchestration.AlivePattern);
        
        var isAlive = regex.IsMatch(query.Name);
        return isAlive;
    }

    #endregion
}