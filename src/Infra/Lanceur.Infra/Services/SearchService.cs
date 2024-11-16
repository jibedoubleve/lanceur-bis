using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class SearchService : SearchServiceCache, ISearchService
{
    private readonly ISearchServiceOrchestrator _orchestrator;

    #region Fields

    private readonly IMacroManager _macroManager;
    private readonly IThumbnailManager _thumbnailManager;
    private readonly ILogger<SearchService> _logger;

    #endregion Fields

    #region Constructors

    public SearchService(
        IStoreLoader storeLoader,
        IMacroManager macroManager,
        IThumbnailManager thumbnailManager,
        ILoggerFactory loggerFactory,
        ISearchServiceOrchestrator orchestrator
    ) : base(storeLoader)
    {
        _macroManager = macroManager;
        _thumbnailManager = thumbnailManager;
        _orchestrator = orchestrator;
        _logger = loggerFactory.GetLogger<SearchService>();
    }

    #endregion Constructors

    #region Methods

    private IEnumerable<QueryResult> SetupAndSort(QueryResult[] input)
    {
        // Upgrade alias to executable macro and return the result
        var results = input?.Any() ?? false
            ? _macroManager.Handle(input).ToList()
            : new();

        // Refresh the thumbnails
        _thumbnailManager.RefreshThumbnails(results);

        // Order the list and return the result
        var orderedResults = results.OrderByDescending(e => e.Count)
                                    .ThenBy(e => e.Name)
                                    .ToArray();

        return input?.Any() ?? false
            ? orderedResults
            : DisplayQueryResult.NoResultFound;
    }

    public async Task<IEnumerable<QueryResult>> GetAllAsync()
    {
        using var measurement = _logger.MeasureExecutionTime(this);

        var tasks = Stores.Select(store => Task.Run(store.GetAll));
        var results = (await Task.WhenAll(tasks))
                      .SelectMany(x => x)
                      .ToArray();

        return SetupAndSort(results);
    }

    public async Task<IEnumerable<QueryResult>> SearchAsync(Cmdline query, bool doesReturnAllIfEmpty = false)
    {
        using var measurement = _logger.MeasureExecutionTime(this);

        if (doesReturnAllIfEmpty && query is null) return await GetAllAsync();
        if (query is null || query.IsEmpty) return new List<QueryResult>();

        //Get the alive stores
        var aliveStores = Stores.Where(service => _orchestrator.IsAlive(service, query))
                                .ToArray();
        var tasks = new List<Task<IEnumerable<QueryResult>>>();

        // I've got a services that stunt all the others, then
        // I execute the search for this one only
        if (aliveStores.Any(x => x.Orchestration.IdleOthers))
        {
            var store = aliveStores.First(x => x.Orchestration.IdleOthers);
            tasks.Add(Task.Run(() => store.Search(query)));
        }
        // No store that stunt all the other stores, execute aggregated search
        else
        {
            tasks = aliveStores.Select(store => Task.Run(() => store.Search(query)))
                               .ToList();
        }

        _logger.LogTrace(
            "For the query '{Query}', {IdleCount} store(s) IDLE and {ActiveCount} store(s) ALIVE",
            query,
            Stores.Count() - tasks.Count,
            tasks.Count
        );

        var results = (await Task.WhenAll(tasks)).SelectMany(x => x).ToArray();


        // Remember the query
        foreach (var result in results) result.Query = query;

        // If there's an exact match, promote it to the top
        // of the list.
        var orderedResults = SetupAndSort(results).ToList();
        var match = orderedResults.FirstOrDefault(r => r.Name == query.Name);
        if (match is not null) orderedResults.Move(match, 0);

        return !orderedResults.Any()
            ? DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline")
            : orderedResults;
    }

    #endregion Methods
}