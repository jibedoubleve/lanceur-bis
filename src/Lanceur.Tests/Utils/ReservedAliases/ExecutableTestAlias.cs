using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    [ReservedAlias("anothertest"), Description("description")]
    public class ExecutableTestAlias : ExecutableQueryResult
    {
        #region Methods

        public static ExecutableTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            Parameters = cmdline.Parameters;
            return NoResultAsync;
        }

        #endregion Methods
    }
}