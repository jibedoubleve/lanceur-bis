using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Infra.Constants;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("logs")]
[Description("Open the directory containing the log files")]
public class ShowLogs : SelfExecutableQueryResult
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public ShowLogs(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    #endregion

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