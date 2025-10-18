using Lanceur.Core.Models;

namespace Lanceur.Core.Requests;

public record ExecutionRequest
{
    #region Constructors

    public ExecutionRequest(QueryResult queryResult) : this(queryResult, queryResult.OriginatingQuery) { }

    public ExecutionRequest(QueryResult queryResult, Cmdline originatingQuery, bool executeWithPrivilege = false)
    {
        QueryResult = queryResult;
        queryResult.OriginatingQuery = originatingQuery;
        ExecuteWithPrivilege = executeWithPrivilege;
    }

    #endregion

    #region Properties

    public bool ExecuteWithPrivilege { get; }

    public QueryResult QueryResult { get;  }

    #endregion
}