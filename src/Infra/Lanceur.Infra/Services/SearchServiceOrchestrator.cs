using System.Text.RegularExpressions;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class SearchServiceOrchestrator : ISearchServiceOrchestrator
{
    #region Fields

    private readonly ILogger<SearchServiceOrchestrator> _log;

    private readonly Dictionary<string, IStoreService> _stores = new();

    #endregion

    #region Constructors

    public SearchServiceOrchestrator(ILoggerFactory factory)
    {
        _log = factory.GetLogger<SearchServiceOrchestrator>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool IsAlive(IStoreService storeService, Cmdline query)
    {
        var regex = new Regex(storeService.Orchestration.AlivePattern);
        var isAlive = regex.IsMatch(query.Name);
        return isAlive;
    }

    #endregion
}