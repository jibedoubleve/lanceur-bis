using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public sealed class SearchService : ISearchService
{
    #region Fields

    private Cmdline? _previousQuery;

    private readonly ILogger<SearchService> _logger;
    private readonly IMacroAliasExpanderService _macroAliasExpanderService;
    private readonly IReadOnlyCollection<IStoreService> _storeServices;

    #endregion

    #region Constructors

    public SearchService(
        IEnumerable<IStoreService> storeServices,
        IMacroAliasExpanderService macroAliasExpanderService,
        ILogger<SearchService> logger
    )
    {
        ArgumentNullException.ThrowIfNull(storeServices);

        _storeServices = storeServices.ToList();
        if (_storeServices.Count == 0)
        {
            throw new ArgumentException("There are no store activated for the search service");
        }

        _macroAliasExpanderService = macroAliasExpanderService;
        _logger = logger;
    }

    #endregion

    #region Methods

    private bool CanPrune(Cmdline? previousQuery, Cmdline currentQuery)
    {
        if (previousQuery is null) { return false; }

        if (previousQuery.IsEmpty()) { return false; }

        return GetAliveStores(currentQuery)
            .All(store => store.CanPruneResult(previousQuery, currentQuery));
    }

    private async Task DispatchSearchAsync(IList<QueryResult> destination, Cmdline currentQuery, bool doesReturnAllIfEmpty)
    {
        using var measurement = _logger.WarnIfSlow(this);
        if (doesReturnAllIfEmpty && currentQuery.IsEmpty())
        {
            destination.ReplaceWith(
                await GetAllAsync()
            );
            return;
        }

        if (currentQuery.IsEmpty())
        {
            destination.Clear();
            return;
        }

        if (CanPrune(_previousQuery, currentQuery))
        {
            _logger.LogTrace(
                "Query {NewQuery} refines {OldQuery}; pruning existing results instead of searching stores",
                currentQuery.ToString(),
                _previousQuery?.ToString() ?? "<EMPTY>"
            );
            
            //CanPrune guarantees the _previousQuery is not null
            PruneResults(destination, _previousQuery!, currentQuery);
            return;
        }

        _logger.LogTrace(
            "Execute a full search for query {Query}",
            currentQuery
        );
        await SearchInStoreAsync(destination, currentQuery);
    }

    private IEnumerable<QueryResult> FormatForDisplay(QueryResult[] collection)
    {
        // Upgrade alias to executable macros (if any) 
        var macros = collection.Length != 0
            ? _macroAliasExpanderService.Expand(collection).ToList()
            : [];

        // Then order the list 
        var orderedResults = macros.OrderByDescending(e => e.Count)
                                   .ThenBy(e => e.Name)
                                   .ToArray();

        //Then return
        return collection.Length != 0
            ? orderedResults
            : DisplayQueryResult.NoResultFound;
    }

    private IStoreService[] GetAliveStores(Cmdline query)
        => _storeServices.Where(service => service.Orchestration.IsAlive(query))
                         .ToArray();

    private void PruneResults(IList<QueryResult> destination, Cmdline previousQuery, Cmdline currentQuery)
    {
        var stores = GetAliveStores(currentQuery);

        var idle = stores.FirstOrDefault(s => s.Orchestration.IdleOthers);
        if (idle is not null)
        {
            stores = [idle];
        }

        var deletedCount = stores.Sum(store => store.PruneResult(destination, previousQuery, currentQuery));

        _logger.LogTrace(
            "Pruned {ItemCount} result(s) for {Query}",
            deletedCount,
            currentQuery
        );
    }

    private async Task SearchInStoreAsync(IList<QueryResult> destination, Cmdline query)
    {
        //Get the alive stores
        var aliveStores = GetAliveStores(query);

        // I've got a service that idles all the others, then
        // I execute the search for this one only
        var tasks = new List<Task<IEnumerable<QueryResult>>>();
        if (aliveStores.Any(x => x.Orchestration.IdleOthers))
        {
            var store = aliveStores.First(x => x.Orchestration.IdleOthers);
            tasks.Add(Task.Run(() => store.Search(query)));
        }
        else // No store that idles all the other stores, execute aggregated search
        {
            tasks = aliveStores.Select(store => Task.Run(() => store.Search(query)))
                               .ToList();
        }

        _logger.LogTrace(
            "For the query {Query}, {IdleCount} store(s) IDLE and {ActiveCount} store(s) ALIVE",
            query,
            _storeServices.Count - tasks.Count,
            tasks.Count
        );

        var results = (await Task.WhenAll(tasks)).SelectMany(x => x).ToArray();

        // Remember the query
        foreach (var result in results)
        {
            result.OriginatingQuery = query;
        }

        var orderedResults = FormatForDisplay(results).ToList();

        // If there's an exact match, promote
        // it to the top of the list.
        var match = orderedResults.FirstOrDefault(r =>
            r.Name.Equals(query.Name, StringComparison.InvariantCultureIgnoreCase)
        );

        if (match is not null) { orderedResults.Move(match, 0); }

        destination.ReplaceWith(
            orderedResults.Count == 0
                ? DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline")
                : orderedResults
        );
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> GetAllAsync()
    {
        using var measurement = _logger.WarnIfSlow(this);

        var tasks = _storeServices.Select(store => Task.Run(store.GetAll));
        var results = (await Task.WhenAll(tasks))
                      .SelectMany(x => x)
                      .ToArray();

        return FormatForDisplay(results);
    }

    /// <inheritdoc />
    public async Task SearchAsync(IList<QueryResult> destination, Cmdline query, bool doesReturnAllIfEmpty = false)
    {
        await DispatchSearchAsync(destination, query, doesReturnAllIfEmpty);
        _previousQuery = query;
    }

    #endregion
}