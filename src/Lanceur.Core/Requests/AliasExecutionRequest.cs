using Lanceur.Core.Models;

namespace Lanceur.Core.Requests
{
    public class AliasExecutionRequest
    {
        #region Properties

        public QueryResult AliasToExecute { get; set; }
        public string Query { get; set; }
        public bool RunAsAdmin { get; set; }

        #endregion Properties

    }
}