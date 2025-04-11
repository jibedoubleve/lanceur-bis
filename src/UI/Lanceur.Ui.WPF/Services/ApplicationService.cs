using System.Windows;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;

namespace Lanceur.Ui.WPF.Services;

public class ApplicationService : IApplicationService
{
    #region Methods

    public Coordinate GetCenterCoordinate() => Application.Current.MainWindow!.GetCenterCoordinate();

    public void Shutdown() => Application.Current.Shutdown();

    #endregion
}