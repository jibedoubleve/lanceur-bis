using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Ui.Core.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Coordinate = Lanceur.Core.Models.Coordinate;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("centre")]
[Description("Centre Lanceur in the middle of the screen.")]
public class CentreAlias : SelfExecutableQueryResult
{
    private readonly ILogger<CentreAlias> _logger;
    private readonly IDatabaseConfigurationService? _appConfig;

    #region Constructors

    public CentreAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var factory = serviceProvider.GetService<ILoggerFactory>() ?? throw new NullReferenceException("Logger factory is ont configured in the service provider");
        _logger = factory.CreateLogger<CentreAlias>();
        _appConfig = serviceProvider.GetService<IDatabaseConfigurationService>() ?? throw new NullReferenceException(nameof(_appConfig));
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
        var coordinate = Application.Current.MainWindow!.GetCenterCoordinate();
        _logger.LogInformation(
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