using System.ComponentModel;
using System.Windows;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("quit")]
[Description("Quit lanceur")]
public class QuitAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<QuitAlias> _logger;

    #endregion

    #region Constructors

    public QuitAlias(ILoggerFactory loggerFactory) => _logger = loggerFactory.GetLogger<QuitAlias>();

    #endregion

    #region Properties

    public override string Icon => "ArrowExit20";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        Application.Current.Shutdown();
        _logger.LogInformation("Quit the application from alias Quit");

        return NoResultAsync;
    }

    #endregion
}