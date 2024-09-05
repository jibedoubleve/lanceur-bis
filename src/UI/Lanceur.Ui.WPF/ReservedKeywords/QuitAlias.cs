using System.ComponentModel;
using System.Windows;
using Lanceur.Core;
using Lanceur.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("quit"), Description("Quit lanceur")]
public class QuitAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<QuitAlias> _logger;

    #endregion

    #region Constructors

    public QuitAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(nameof(serviceProvider));

        _logger = serviceProvider.GetService<ILogger<QuitAlias>>() ?? throw new ArgumentNullException(nameof(_logger));
    }

    #endregion

    #region Properties

    public override string Icon => "LocationExit";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        Application.Current.Shutdown();
        _logger.LogInformation("Quit the application from alias 'Quit'");

        return NoResultAsync;
    }

    #endregion
}