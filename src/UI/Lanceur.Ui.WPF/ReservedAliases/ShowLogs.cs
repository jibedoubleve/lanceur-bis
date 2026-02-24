using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;

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
        Process.Start("explorer.exe", Paths.LogRepository);
        return NoResultAsync;
    }

    #endregion
}