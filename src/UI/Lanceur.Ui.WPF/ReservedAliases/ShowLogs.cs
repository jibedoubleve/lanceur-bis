using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Infra.Win32.Helpers;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("logs")]
[Description("Open the directory containing the log files")]
public class ShowLogs : SelfExecutableQueryResult
{
    #region Properties

    public override string Icon => "Book24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        WindowsShell.StartExplorer(Paths.LogRepository);
        return NoResultAsync;
    }

    #endregion
}