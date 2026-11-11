using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("quit")]
[Description("Quit lanceur")]
public sealed class QuitAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<QuitAlias> _logger;
    private readonly IApplicationShellService _shell;

    #endregion

    #region Constructors

    public QuitAlias(ILoggerFactory loggerFactory, IApplicationShellService shell)
    {
        _logger = loggerFactory.CreateLogger<QuitAlias>();
        _shell = shell;
    }

    #endregion

    #region Properties

    public override string Icon => "ArrowExit20";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _shell.Shutdown();
        _logger.LogInformation("Quit the application from alias Quit");

        return NoResultAsync;
    }

    #endregion
}