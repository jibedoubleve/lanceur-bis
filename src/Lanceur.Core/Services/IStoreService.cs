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
    ///     Get all the results of this search service.
    /// </summary>
    IEnumerable<QueryResult> GetAll();

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