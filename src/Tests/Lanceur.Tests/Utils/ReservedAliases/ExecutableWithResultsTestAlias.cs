using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    public class ExecutableWithResultsTestAlias : MacroQueryResult
    {
        #region Methods

        public static ExecutableWithResultsTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };

        public override SelfExecutableQueryResult Clone() => this.CloneObject();

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