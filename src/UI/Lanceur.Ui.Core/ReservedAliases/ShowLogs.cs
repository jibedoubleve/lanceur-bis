using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("logs")]
[Description("Open the directory containing the log files")]
public sealed class ShowLogs : SelfExecutableQueryResult
{
    #region Fields

    private readonly IProcessLauncher _processLauncher;

    #endregion

    #region Constructors

    public ShowLogs(IProcessLauncher processLauncher) => _processLauncher = processLauncher;

    #endregion

    #region Properties

    public override string Icon => "Book24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _processLauncher.Open(Paths.LogRepository);
        return NoResultAsync;
    }

    #endregion
}