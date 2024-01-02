using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Macros.Development;
using Xunit;

namespace Lanceur.Tests.Utils
{
    internal class DebugMacroExecutor : IExecutionManager
    {
        #region Methods

        public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
        {
            if (request.QueryResult is DebugMacro macro)
            {
                return new()
                {
                    Results = await macro.ExecuteAsync(new("debug")),
                };
            }
            else
            {
                Assert.True(false, "Request not containing a 'DebugMacro'");
                return null;
            }
        }

        #endregion Methods
    }
}