using Lanceur.Core.Models;
using Lanceur.Core.Requests;

namespace Lanceur.Core.Managers
{
    public interface IExecutionManager
    {
        #region Methods

        Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request);

        ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0);

        #endregion Methods
    }
}