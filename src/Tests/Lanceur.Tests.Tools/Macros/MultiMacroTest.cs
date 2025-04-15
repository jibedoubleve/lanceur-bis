using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Infra.Macros;

namespace Lanceur.Tests.Tools.Macros;

[Macro("multi"), Description("Allow to start multiple alias at once")]
public class MultiMacroTest : MacroQueryResult
{
    private readonly IServiceProvider _serviceProvider;

    #region Constructors

    public MultiMacroTest(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

    #endregion Constructors

    #region Methods

    public override SelfExecutableQueryResult Clone() => new MultiMacro(_serviceProvider) { Description = "Start multiple aliases at once." };

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        cmdline ??= Cmdline.Empty;
        var list = new List<QueryResult> { new DisplayQueryResult(cmdline.Name, cmdline.Parameters) };
        return Task.FromResult<IEnumerable<QueryResult>>(list);
    }

    #endregion Methods
}