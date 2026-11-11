using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public sealed class SearchService : ISearchService
{
    #region Fields

    private readonly ILogger<SearchService> _logger;
    private readonly IMacroAliasExpanderService _macroAliasExpanderService;

    private Cmdline? _previousQuery;
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

        // Pruning only makes sense on results produced by the very same stores. When the set of
        // alive stores changes (e.g. the user just finished typing an alphanumeric store shortcut),
        // the current result set comes from other stores and a full search is required.
        var currentStores = GetAliveStores(currentQuery).ToHashSet();
        var previousStores = GetAliveStores(previousQuery).ToHashSet();

        return currentStores.SetEquals(previousStores) &&
               currentStores.All(store => store.CanPruneResult(previousQuery, currentQuery));
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
    {
        var stores = _storeServices.Where(service => service.Orchestration.IsAlive(query))
                                   .ToArray();

        return stores.Any(x => x.Orchestration.IdleOthers)
            ? [stores.First(x => x.Orchestration.IdleOthers)]
            : stores;
    }

    private async Task<IEnumerable<QueryResult>> GetAllAsync()
    {
        using var measurement = _logger.WarnIfSlow(this);

        var tasks = _storeServices.Select(store => Task.Run(store.GetAll));
        var results = (await Task.WhenAll(tasks))
                      .SelectMany(x => x)
                      .ToArray();

        return FormatForDisplay(results);
    }

    private void PruneResults(IList<QueryResult> destination, Cmdline previousQuery, Cmdline currentQuery)
    {
        var deletedCount = GetAliveStores(currentQuery)
            .Sum(store => store.PruneResult(destination, previousQuery, currentQuery));

        _logger.LogTrace(
            "Pruned {ItemCount} result(s) for {Query}",
            deletedCount,
            currentQuery
        );
    }

    private async Task SearchInStoreAsync(IList<QueryResult> destination, Cmdline query)
    {
        var tasks = GetAliveStores(query).Select(store => Task.Run(() => store.Search(query)))
                                         .ToList();

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
    public async Task SearchAsync(IList<QueryResult> destination, Cmdline query, bool doesReturnAllIfEmpty = false)
    {
        using var measurement = _logger.WarnIfSlow(this);
        if (doesReturnAllIfEmpty && query.IsEmpty())
        {
            destination.ReplaceWith(
                await GetAllAsync()
            );
            return;
        }

        if (query.IsEmpty())
        {
            destination.Clear();
            return;
        }

        if (CanPrune(_previousQuery, query))
        {
            _logger.LogTrace(
                "Query {NewQuery} refines {OldQuery}; pruning existing results instead of searching stores",
                query.ToString(),
                _previousQuery?.ToString() ?? "<EMPTY>"
            );

            //CanPrune guarantees the _previousQuery is not null
            PruneResults(destination, _previousQuery!, query);
            return;
        }

        _logger.LogTrace(
            "Execute a full search for query {Query}",
            query
        );
        await SearchInStoreAsync(destination, query);

        _previousQuery = query;
    }

    #endregion
}