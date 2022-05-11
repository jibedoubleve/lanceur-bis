using Lanceur.Core.Models;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    public class ExecutableWithResultsTestAlias : ExecutableQueryResult
    {
        #region Methods

        public static ExecutableWithResultsTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var result = new List<QueryResult>()
            {
                FromName("name1"),
                FromName("name2"),
            };
            return Task.FromResult((IEnumerable<QueryResult>)result);
        }

        #endregion Methods
    }
}