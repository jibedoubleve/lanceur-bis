using Lanceur.Core.Models;

namespace Lanceur.Core
{
    public interface IExecutable
    {
        #region Methods

        Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null);

        #endregion Methods
    }
}