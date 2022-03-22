using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class RepositoryStore : ISearchService
    {
        #region Methods

        public IEnumerable<QueryResult> GetAll()
        {
            return new List<QueryResult>();
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            return new List<QueryResult>();
        }

        #endregion Methods
    }
}