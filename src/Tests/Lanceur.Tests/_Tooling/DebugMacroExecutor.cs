using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Macros.Development;
using Xunit;

namespace Lanceur.Tests.Tooling;

internal class DebugMacroExecutor : IExecutionManager
{
    #region Methods

    public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
    {
        if (request.QueryResult is DebugMacro macro) return new() { Results = await macro.ExecuteAsync(new("debug")) };

        Assert.True(false, "Request not containing a 'DebugMacro'");
        return null;
    }

    public ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0) => throw new NotImplementedException();

    #endregion Methods
}