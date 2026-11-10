using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Provides search functionality across all registered stores in the system.
/// </summary>
public interface ISearchService
{
    #region Methods

    /// <summary>
    ///     Executes a search query across all registered stores and populates the provided collection with the results.
    /// </summary>
    /// <param name="destination">The collection to populate with search results.</param>
    /// <param name="query">The search query parameters.</param>
    /// <param name="doesReturnAllIfEmpty">If true, returns all available results when the query is empty.</param>
    Task SearchAsync(IList<QueryResult> destination, Cmdline query, bool doesReturnAllIfEmpty = false);

    #endregion
}