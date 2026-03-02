using Lanceur.Core.Models;

namespace Lanceur.Core.Responses;

public class ExecutionResponse
{
    #region Properties

    public bool HasResult { get; init; }

    public static ExecutionResponse NoResult => new() { HasResult = false, Results = new List<QueryResult>() };

    public IEnumerable<QueryResult> Results { get; set; }

    #endregion

    #region Methods

    public static ExecutionResponse FromResults(IEnumerable<QueryResult> results)
    {
        results = results?.ToArray() ?? [];
        return new() { Results = results, HasResult = results.Any() };
    }

    #endregion
}