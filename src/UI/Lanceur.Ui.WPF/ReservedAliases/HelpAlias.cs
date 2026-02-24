using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("help")]
[Description("Open help page")]
public class HelpAlias : SelfExecutableQueryResult
{
    #region Fields

    private const string Url = "https://jibedoubleve.github.io/lanceur-bis/";

    #endregion

    #region Properties

    public override string Icon => "ChatHelp24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        Process.Start("explorer.exe", Url);
        return NoResultAsync;
    }

    #endregion
}