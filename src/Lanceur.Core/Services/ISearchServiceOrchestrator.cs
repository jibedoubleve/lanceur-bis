using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface ISearchServiceOrchestrator
{
    /// <summary>
    /// Indicates whether the search service should be idle and skip this query or
    /// execute the search
    /// </summary>
    /// <param name="searchService">The search service to tests</param>
    /// <param name="query">To query to be executed</param>
    /// <returns><c>True</c> if the search should be executes; otherwise <c>False</c></returns>
    bool IsAlive(ISearchService searchService, Cmdline query);

    /// <summary>
    /// Registers a store to be orchestrated.
    /// </summary>
    /// <param name="services">The search services to be registered for orchestration.</param>
    void Register(params ISearchService[] services);
}