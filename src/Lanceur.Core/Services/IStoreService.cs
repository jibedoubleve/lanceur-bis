using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IStoreService
{
    #region Properties

    /// <summary>
    ///     Gets a value indicating whether the store's shortcut can be overridden
    ///     by the user through the configuration settings.
    /// </summary>
    bool IsOverridable { get; }

    /// <summary>
    ///     Configuration of the orchestration.
    /// </summary>
    StoreOrchestration StoreOrchestration { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Determines whether the current result set can be refined by pruning
    ///     instead of executing a full search again.
    /// </summary>
    /// <remarks>
    ///     Returns <c>true</c> when the new query is a logical refinement of the
    ///     previous one and the store's search semantics guarantee that the results
    ///     of <paramref name="current" /> are a subset of the results of
    ///     <paramref name="previous" />. In that case, <see cref="PruneResult" /> can
    ///     be called instead of <see cref="Search" />.
    /// </remarks>
    /// <param name="previous">The query used to produce the current result set.</param>
    /// <param name="current">The new query to evaluate.</param>
    /// <returns>
    ///     <c>true</c> if the existing results can be pruned to satisfy
    ///     <paramref name="current" />; <c>false</c> if a full search is required.
    /// </returns>
    bool CanPruneResult(Cmdline previous, Cmdline current);

    /// <summary>
    ///     Get all the results of this search service.
    /// </summary>
    IEnumerable<QueryResult> GetAll();

    /// <summary>
    ///     Removes from <paramref name="destination" /> any result that no longer
    ///     satisfies <paramref name="current" />, according to this store's search
    ///     semantics.
    /// </summary>
    /// <remarks>
    ///     This method is only called when <see cref="CanPruneResult" /> returned
    ///     <c>true</c>. Each store is responsible for applying its own filtering
    ///     logic (e.g. prefix match on <see cref="Cmdline.Name" />, substring match
    ///     on <see cref="Cmdline.Parameters" />). The list is modified in place.
    /// </remarks>
    /// <param name="destination">The current result set, modified in place.</param>
    /// <param name="current">The new query to filter against.</param>
    /// <param name="previous">
    ///     The query that produced the current result set. Used to determine which items this store
    ///     originally contributed, so that only those items are subject to removal.
    /// </param>
    /// <returns>The number of items removed from <paramref name="destination" />.</returns>
    int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current);

    /// <summary>
    ///     Execute a search with the specified query
    /// </summary>
    /// <param name="cmdline">The query for the search</param>
    /// <returns>
    ///     The result of the search or empty <see cref="IEnumerable{T}" /> if idle or no results.
    /// </returns>
    IEnumerable<QueryResult> Search(Cmdline cmdline);

    #endregion
}