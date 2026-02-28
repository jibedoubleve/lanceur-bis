using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface ISearchServiceOrchestrator
{
    #region Methods

    /// <summary>
    ///     Indicates whether the search service should be idle and skip this query or
    ///     execute the search
    /// </summary>
    /// <param name="storeService">The search service to tests</param>
    /// <param name="query">To query to be executed</param>
    /// <returns><c>True</c> if the search should be executes; otherwise <c>False</c></returns>
    bool IsAlive(IStoreService storeService, Cmdline query);

    #endregion
}