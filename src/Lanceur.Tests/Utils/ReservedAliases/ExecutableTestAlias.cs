using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    [ReservedAlias("anothertest"), Description("description")]
    public class ExecutableTestAlias : ExecutableQueryResult
    {
        #region Properties

        public string Parameters { get; private set; }

        #endregion Properties

        #region Methods

        public static ExecutableTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string parameter = null)
        {
            Parameters = parameter;
            return NoResultAsync;
        }

        #endregion Methods
    }
}