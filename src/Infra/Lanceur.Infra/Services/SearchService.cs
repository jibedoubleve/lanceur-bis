using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public sealed class SearchService : ISearchService
{
    #region Fields

    private Cmdline? _lastQuery;

    private readonly ILogger<SearchService> _logger;
    private readonly IMacroAliasExpanderService _macroAliasExpanderService;
    private readonly ISearchServiceOrchestrator _orchestrator;
    private readonly IEnumerable<IStoreService> _storeServices;

    #endregion

    #region Constructors

    public SearchService(
        IEnumerable<IStoreService> storeServices,
        IMacroAliasExpanderService macroAliasExpanderService,
        ILogger<SearchService> logger,
        ISearchServiceOrchestrator orchestrator
    )
    {
        ArgumentNullException.ThrowIfNull(storeServices);

        _storeServices = storeServices.ToList();
        if (!_storeServices.Any())
        {
            throw new ArgumentException("There are no store activated for the search service");
        }

        _macroAliasExpanderService = macroAliasExpanderService;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    #endregion

    #region Methods

    private void PruneResults(IList<QueryResult> destination, Cmdline query)
    {
        var toDelete = destination.Where(item =>
            !item.Name.StartsWith(query.Parameters, StringComparison.InvariantCultureIgnoreCase)
        ).ToArray();

        _logger.LogTrace(
            "Prune {ItemCount} result(s) for {Query}",
            toDelete.Length,
            query
        );
        
        destination.RemoveMultiple(toDelete);
    }

    private IEnumerable<QueryResult> FormatForDisplay(QueryResult[] collection)
    {
        collection ??= [];
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

    private async Task SearchInStoreAsync(IList<QueryResult> destination, Cmdline query)
    {
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
        foreach (var result in results)
        {
            result.OriginatingQuery = query;
        }

        var orderedResults = FormatForDisplay(results).ToList();

        // If there's an exact match, promote
        // it to the top of the list.
        var match = orderedResults.FirstOrDefault(r => r.Name == query.Name);
        if (match is not null) { orderedResults.Move(match, 0); }

        if (orderedResults.Count == 0)
        {
            destination.ReplaceWith(
                DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline")
            );
        }
        else { destination.ReplaceWith(orderedResults); }
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
        using var measurement = _logger.WarnIfSlow(this);

        if (doesReturnAllIfEmpty && query.IsEmpty())
        {
            destination.ReplaceWith(
                await GetAllAsync()
            );
            return;
        }

        if (query.IsEmpty()) { destination.Clear(); }

        if (_lastQuery is not null 
            && query.Parameters.Contains(_lastQuery.Parameters, StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogTrace(
                "Query {NewQuery} refines {OldQuery}; pruning existing results instead of searching stores",
                query.Parameters,
                _lastQuery.Parameters
            );
            PruneResults(destination, query);
            return;
        }

        _logger.LogTrace(
            "Execute a full search for query {Query}",
            query
        );
        await SearchInStoreAsync(destination, query);

        _lastQuery = query;
    }

    #endregion
}