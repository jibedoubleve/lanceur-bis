using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Tests.Tooling.ReservedAliases;

namespace Lanceur.Tests.Tools.ReservedAliases;

[ReservedAlias(Names.Name1)]
[Description("description")]
public class ExecutableTestAlias : MacroQueryResult
{
    #region Methods

    public override SelfExecutableQueryResult Clone() => new ExecutableTestAlias();

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        Parameters = cmdline?.Parameters;
        return NoResultAsync;
    }

    #endregion
}