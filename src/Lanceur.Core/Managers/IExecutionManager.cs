﻿using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;

namespace Lanceur.Core.Managers;

public interface IExecutionManager
{
    #region Methods

    Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request);

    ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0);

    #endregion Methods
}