using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
/// Provides search functionality across all registered stores in the system.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Executes a search query across all registered stores.
    /// </summary>
    /// <param name="query">The search query parameters.</param>
    /// <param name="doesReturnAllIfEmpty">If true, returns all available results when the query is empty.</param>
    /// <returns>A task that resolves to a collection of search results.</returns>
    Task<IEnumerable<QueryResult>> SearchAsync(Cmdline query, bool doesReturnAllIfEmpty = false);

    /// <summary>
    /// Retrieves all available results from all registered stores without applying any search filters.
    /// </summary>
    /// <returns>A task that resolves to a collection of all available results.</returns>
    Task<IEnumerable<QueryResult>> GetAllAsync();
}