using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Ui.Core.Messages;
using Microsoft.Extensions.Logging;
using Coordinate = Lanceur.Core.Models.Coordinate;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("centre")]
[Description("Centre Lanceur in the middle of the screen.")]
public sealed class CentreAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<CentreAlias> _logger;

    private readonly IWriteableSection<WindowSection> _settings;

    #endregion

    #region Constructors

    public CentreAlias(ILoggerFactory loggerFactory, IWriteableSection<WindowSection> settings)
    {
        _logger = loggerFactory.CreateLogger<CentreAlias>();
        _settings = settings;
    }

    #endregion

    #region Properties

    public override string Icon => "CenterHorizontal24";

    #endregion

    #region Methods

    private void Save(Coordinate coordinate)
    {
        _settings.Value.Position.Left = coordinate.X;
        _settings.Value.Position.Top = coordinate.Y;
        _settings.Save();
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