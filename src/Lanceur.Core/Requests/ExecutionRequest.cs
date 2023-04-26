using Lanceur.Core.Models;

namespace Lanceur.Core.Requests
{
    public class ExecutionRequest
    {
        #region Properties
        public string Query { get; set; }
        public bool ExecuteWithPrivilege { get; set; }
        public QueryResult QueryResult { get; set; }

        #endregion Properties
    }
}