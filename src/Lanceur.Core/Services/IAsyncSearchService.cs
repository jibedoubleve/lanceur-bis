using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IAsyncSearchService
{
    Task<IEnumerable<QueryResult>> SearchAsync(Cmdline query);
    Task<IEnumerable<QueryResult>> GetAllAsync();
}