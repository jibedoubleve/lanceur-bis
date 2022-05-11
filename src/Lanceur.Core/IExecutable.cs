using Lanceur.Core.Models;

namespace Lanceur.Core
{
    public interface IExecutable
    {
        #region Methods

        Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

        #endregion Methods
    }
}