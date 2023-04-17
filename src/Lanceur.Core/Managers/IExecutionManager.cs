using Lanceur.Core.Models;

namespace Lanceur.Core.Managers
{
    public interface IExecutionManager
    {
        #region Methods

        Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request);

        #endregion Methods
    }

    public class ExecutionRequest
    {
        #region Properties
        public string Query { get; set; }
        public bool ExecuteWithPrivilege { get; set; }
        public QueryResult QueryResult { get; set; }

        #endregion Properties
    }

    public class ExecutionResponse
    {
        #region Properties

        public static ExecutionResponse EmptyResult => new()
        {
            HasResult = true,
            Results = new List<QueryResult>()
        };

        public static ExecutionResponse NoResult => new()
        {
            HasResult = false,
            Results = new List<QueryResult>()
        };

        public bool HasResult { get; set; }

        public IEnumerable<QueryResult> Results { get; set; }

        #endregion Properties

        #region Methods

        public static ExecutionResponse FromResults(IEnumerable<QueryResult> results)
        {
            return new ExecutionResponse
            {
                Results = results,
                HasResult = results.Any(),
            };
        }

        #endregion Methods
    }
}