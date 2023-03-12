using Lanceur.Core.Models;

namespace Lanceur.Core.Services
{
    public interface ISearchService
    {
        #region Methods

        IEnumerable<QueryResult> GetAll();

        IEnumerable<QueryResult> Search(string query);

        #endregion Methods
    }
}