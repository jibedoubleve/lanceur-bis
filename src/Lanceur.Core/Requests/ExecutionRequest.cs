using Lanceur.Core.Models;

namespace Lanceur.Core.Requests;

public record ExecutionRequest
{
    #region Properties

    public bool ExecuteWithPrivilege { get; init; }
    public string Query { get; init; }
    public QueryResult QueryResult { get; init; }

    #endregion Properties
}