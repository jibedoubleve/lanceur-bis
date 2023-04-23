using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    [ReservedAlias("anothertest"), Description("description")]
    public class ExecutableTestAlias : ExecutableQueryResult
    {
        #region Constructors

        public ExecutableTestAlias()
        {
            Name = Guid.NewGuid().ToString().Substring(0, 8);
        }

        #endregion Constructors

        #region Methods

        public static ExecutableTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };

        public static ExecutableTestAlias Random() => FromName(Guid.NewGuid().ToString().Substring(0, 8));

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            Parameters = cmdline.Parameters;
            return NoResultAsync;
        }

        #endregion Methods
    }
}