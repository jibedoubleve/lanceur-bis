using System.ComponentModel;
using System.Windows;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("quit")]
[Description("Quit lanceur")]
public class QuitAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<QuitAlias> _logger;

    #endregion

    #region Constructors

    public QuitAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        var factory = serviceProvider.GetService<ILoggerFactory>() ?? throw new NullReferenceException("Log factory is not configured in the service provider.");
        _logger = factory.GetLogger<QuitAlias>();
    }

    #endregion

    #region Properties

    public override string Icon => "ArrowExit20";

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