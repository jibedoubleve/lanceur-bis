using System.Text.RegularExpressions;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Services;

public class SearchServiceOrchestrator : ISearchServiceOrchestrator
{
    #region Fields

    private readonly Dictionary<string, ISearchService> _stores = new();
    private readonly ILogger<SearchServiceOrchestrator> _log;

    #endregion Fields

    public SearchServiceOrchestrator(ILoggerFactory factory = null)
    {
        factory ??= Locator.Current.GetService<ILoggerFactory>();
        _log = factory.GetLogger<SearchServiceOrchestrator>();
    }

    #region Methods

    /// <inheritdoc />
    public bool IsAlive(ISearchService searchService, Cmdline query)
    {
        var regex = new Regex(searchService.Orchestration.AlivePattern);
        var isAlive = regex.IsMatch(query.Name);
        return isAlive;
    }

    #endregion Methods
}