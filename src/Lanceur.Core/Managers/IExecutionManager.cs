using Lanceur.Core.Requests;

namespace Lanceur.Core.Managers
{
    public interface IExecutionManager
    {
        #region Methods

        Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request);

        #endregion Methods
    }
}