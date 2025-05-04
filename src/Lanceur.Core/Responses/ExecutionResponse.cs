using Lanceur.Core.Models;

namespace Lanceur.Core.Responses;

public class ExecutionResponse
{
    #region Properties

    public static ExecutionResponse NoResult => new() { HasResult = false, Results = new List<QueryResult>() };

    public bool HasResult { get; init; }

    public IEnumerable<QueryResult> Results { get; set; }

    #endregion Properties

    #region Methods

    public static ExecutionResponse FromResults(IEnumerable<QueryResult> results)
    {
        results = results?.ToArray() ?? [];
        return new()
        {
            Results = results, 
            HasResult = results.Any() 
        };
    }

    #endregion Methods
}