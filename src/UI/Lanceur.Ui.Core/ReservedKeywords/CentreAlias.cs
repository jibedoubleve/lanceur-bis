using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Coordinate = Lanceur.Core.Models.Coordinate;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("centre")]
[Description("Centre Lanceur in the middle of the screen.")]
public class CentreAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IDatabaseConfigurationService? _appConfig;
    private readonly IApplicationService _application;
    private readonly ILogger<CentreAlias>? _logger;

    #endregion

    #region Constructors

    public CentreAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var factory = serviceProvider.GetService<ILoggerFactory>() ?? throw new NullReferenceException("Logger factory is not configured in the service provider");
        _logger = factory.CreateLogger<CentreAlias>();
        _appConfig = serviceProvider.GetService<IDatabaseConfigurationService>() ?? throw new NullReferenceException($"{nameof(IDatabaseConfigurationService)} is not configured in the service provider)");
        _application = serviceProvider.GetService<IApplicationService>() ?? throw new NullReferenceException($"{nameof(IApplicationService)} is not configured in the service provider");
    }

    #endregion

    #region Properties

    public override string Icon => "CenterHorizontal24";

    #endregion

    #region Methods

    private void Save(Coordinate coordinate)
    {
        _appConfig!.Edit(
            s =>
            {
                s.Window.Position.Left = coordinate.X;
                s.Window.Position.Top = coordinate.Y;
            }
        );
    }

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        var coordinate = _application.GetCenterCoordinate();
        _logger!.LogInformation(
            "Put window at default position. (x: {X} - y: {Y})",
            coordinate.X,
            coordinate.Y
        );
        Save(coordinate);
        WeakReferenceMessenger.Default.Send(new ChangeCoordinateMessage(coordinate));
        return NoResultAsync;
    }

    #endregion
}