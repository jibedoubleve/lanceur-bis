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
        public QueryResult QueryResult { get; set; }
        public Cmdline Cmdline { get; set; }
        public bool ExecuteWithPrivilege { get; set; }

        #endregion Properties
    }

    public class ExecutionResponse
    {
        public IEnumerable<QueryResult> Results { get; set; }
        public bool HasResult { get; set; }

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

    }
}