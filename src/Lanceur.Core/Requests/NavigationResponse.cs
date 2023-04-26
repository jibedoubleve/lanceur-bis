using Lanceur.Core.Models;

namespace Lanceur.Core.Requests
{
    public class NavigationResponse
    {
        #region Properties

        public QueryResult CurrentAlias { get; set; }
        public string Query { get; set; }

        #endregion Properties
    }
}