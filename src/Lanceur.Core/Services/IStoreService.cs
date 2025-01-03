using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IStoreService
{
    #region Methods

    /// <summary>
    /// Configuration of the orchestration.
    /// </summary>
    StoreOrchestration StoreOrchestration { get; }

    /// <summary>
    /// Get all the results of this search service.
    /// </summary>
    IEnumerable<QueryResult> GetAll();

    /// <summary>
    /// Execute a search with the specified query
    /// </summary>
    /// <param name="query">The query for the search</param>
    /// <returns>
    /// The result of the search or empty <see cref="IEnumerable{T}"/> if idle or no results.
    /// </returns>
    IEnumerable<QueryResult> Search(Cmdline query);

    #endregion Methods
}