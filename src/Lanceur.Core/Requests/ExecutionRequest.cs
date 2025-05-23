﻿using Lanceur.Core.Models;

namespace Lanceur.Core.Requests;

public record ExecutionRequest
{
    #region Properties

    public bool ExecuteWithPrivilege { get; init; }

    /// <summary>
    ///     The original query that led to the generation of this result.
    /// </summary>
    //TODO: check whether it is doubloon with QueryResult.OriginatingQuery
    public string OriginatingQuery { get; init; }

    public QueryResult QueryResult { get; init; }

    #endregion
}