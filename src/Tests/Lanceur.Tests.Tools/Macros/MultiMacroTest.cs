using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Infra.Macros;

namespace Lanceur.Tests.Tools.Macros;

[Macro("multi"), Description("Allow to start multiple alias at once")]
public class MultiMacroTest : MacroQueryResult
{
    #region Methods

    public override SelfExecutableQueryResult Clone() => new MultiMacroTest { Description = "Start multiple aliases at once." };

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        cmdline ??= Cmdline.Empty;
        var list = new List<QueryResult> { new DisplayQueryResult(cmdline.Name, cmdline.Parameters) };
        return Task.FromResult<IEnumerable<QueryResult>>(list);
    }

    #endregion Methods
}