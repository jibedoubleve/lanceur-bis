using Lanceur.Core.Models;

namespace Lanceur.Core.Requests
{
    public class AliasResponse
    {
        #region Properties

        public string CurrentSessionName { get; set; }
        public bool KeepAlive { get; set; }
        public IEnumerable<QueryResult> Results { get; set; } = new List<QueryResult>();

        #endregion Properties

        #region Methods

        public static implicit operator Task<AliasResponse>(AliasResponse src) => Task.FromResult(src);

        #endregion Methods
    }
}