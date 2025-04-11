using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("quit")]
[Description("Quit lanceur")]
public class QuitAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IApplicationService _application;

    private readonly ILogger<QuitAlias> _logger;

    #endregion

    #region Constructors

    public QuitAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        var factory = serviceProvider.GetService<ILoggerFactory>() ?? throw new NullReferenceException("Log factory is not configured in the service provider.");
        _application = serviceProvider.GetService<IApplicationService>() ?? throw new NullReferenceException("ApplicationService is not configured in the service provider.");
        _logger = factory.GetLogger<QuitAlias>();
    }

    #endregion

    #region Properties

    public override string Icon => "ArrowExit20";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _logger.LogInformation("Quit the application from alias 'Quit'");
        _application.Shutdown();
        return NoResultAsync;
    }

    #endregion
}