using System.Windows;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public sealed class ApplicationShellService : IApplicationShellService
{
    #region Methods

    /// <inheritdoc />
    public Coordinate GetMainWindowCentre() => Application.Current.MainWindow?.GetCenterCoordinate()
                                               ?? throw new InvalidOperationException(
                                                   "The main window is not available.");

    /// <inheritdoc />
    public void Shutdown() => Application.Current.Shutdown();

    #endregion
}