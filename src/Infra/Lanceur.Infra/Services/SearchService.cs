using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public class SearchService : ISearchService
{
    #region Fields

    private readonly ILogger<SearchService> _logger;
    private readonly IMacroService _macroService;
    private readonly ISearchServiceOrchestrator _orchestrator;
    private readonly IEnumerable<IStoreService> _storeServices;

    #endregion

    #region Constructors

    public SearchService(
        IEnumerable<IStoreService> storeServices,
        IMacroService macroService,
        ILogger<SearchService> logger,
        ISearchServiceOrchestrator orchestrator
    )
    {
        ArgumentNullException.ThrowIfNull(storeServices);

        _storeServices = storeServices.ToList();
        if (!_storeServices.Any())
            throw new ArgumentException($"There are no store activated for the search service");

        _macroService = macroService;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    #endregion

    #region Methods

    private IEnumerable<QueryResult> Sort(QueryResult[] collection)
    {
        collection ??= [];
        // Upgrade alias to an executable macro and return the result
        var results = collection.Length != 0
            ? _macroService.ExpandMacroAlias(collection).ToList()
            : [];

        // Order the list and return the result
        var orderedResults = results.OrderByDescending(e => e.Count)
                                    .ThenBy(e => e.Name)
                                    .ToArray();

        return collection.Length != 0
            ? orderedResults
            : DisplayQueryResult.NoResultFound;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> GetAllAsync()
    {
        using var measurement = _logger.WarnIfSlow(this);

        var tasks = _storeServices.Select(store => Task.Run(store.GetAll));
        var results = (await Task.WhenAll(tasks))
                      .SelectMany(x => x)
                      .ToArray();

        return Sort(results);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> SearchAsync(Cmdline query, bool doesReturnAllIfEmpty = false)
    {
        using var measurement = _logger.WarnIfSlow(this);

        if (doesReturnAllIfEmpty && query.IsEmpty()) return await GetAllAsync();
        if (query.IsEmpty()) return new List<QueryResult>();

        //Get the alive stores
        var aliveStores = _storeServices.Where(service => _orchestrator.IsAlive(service, query))
                                        .ToArray();

        // I've got a service that stunts all the others, then
        // I execute the search for this one only
        var tasks = new List<Task<IEnumerable<QueryResult>>>();
        if (aliveStores.Any(x => x.StoreOrchestration.IdleOthers))
        {
            var store = aliveStores.First(x => x.StoreOrchestration.IdleOthers);
            tasks.Add(Task.Run(() => store.Search(query)));
        }
        else // No store that stunt all the other stores, execute aggregated search
        {
            tasks = aliveStores.Select(store => Task.Run(() => store.Search(query)))
                               .ToList();
        }

        _logger.LogTrace(
            "For the query {Query}, {IdleCount} store(s) IDLE and {ActiveCount} store(s) ALIVE",
            query,
            _storeServices.Count() - tasks.Count,
            tasks.Count
        );

        var results = (await Task.WhenAll(tasks)).SelectMany(x => x).ToArray();

        // Remember the query
        foreach (var result in results) result.OriginatingQuery = query;

        var orderedResults = Sort(results).ToList();

        // If there's an exact match, promote
        // it to the top of the list.
        var match = orderedResults.FirstOrDefault(r => r.Name == query.Name);
        if (match is not null) orderedResults.Move(match, 0);

        return !orderedResults.Any()
            ? DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline")
            : orderedResults;
    }

    #endregion
}