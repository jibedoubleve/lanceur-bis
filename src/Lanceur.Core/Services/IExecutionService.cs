using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;

namespace Lanceur.Core.Services;

public interface IExecutionService
{
    #region Methods

    /// <summary>
    ///     Executes the alias specified in the provided <see cref="ExecutionRequest" />
    ///     and returns the results, if any, encapsulated in an <see cref="ExecutionResponse" />.
    /// </summary>
    /// <remarks>
    ///     This method has a side effect: it updates the counter of the alias specified in the
    ///     <see cref="ExecutionRequest" />. This behaviour may be unexpected and has been
    ///     flagged for future refactoring to improve clarity and maintainability.
    /// </remarks>
    /// <param name="request">
    ///     The <see cref="ExecutionRequest" /> containing the alias to be executed. Must not be null.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{ExecutionResponse}" /> that represents the asynchronous operation,
    ///     with the execution results encapsulated in an <see cref="ExecutionResponse" />.
    /// </returns>
    Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request);

    ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0);

    /// <summary>
    ///     Attempts to open the directory where the executable is located.
    ///     If the provided alias does not correspond to a file, no action is taken.
    /// </summary>
    /// <param name="queryResult">The alias representing the file or directory to be opened.</param>
    void OpenDirectoryAsync(QueryResult queryResult);

    #endregion
}