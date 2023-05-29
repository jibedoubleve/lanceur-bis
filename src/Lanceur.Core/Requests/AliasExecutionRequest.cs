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

        #region Methods

        public static implicit operator AliasExecutionRequest(string query) => new() { Query = query };

        #endregion Methods
    }
}