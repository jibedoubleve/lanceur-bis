using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface ISearchService
{
    Task<IEnumerable<QueryResult>> SearchAsync(Cmdline query, bool doesReturnAllIfEmpty = false);
    Task<IEnumerable<QueryResult>> GetAllAsync();
}