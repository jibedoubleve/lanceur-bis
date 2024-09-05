using System.Windows;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Utils;

public static class ScreenRuler
{
    #region Properties

    private static double WindowHeight => Application.Current.MainWindow!.Height;

    private static double WindowWidth => Application.Current.MainWindow!.Width;

    #endregion

    #region Methods

    public static Coordinate GetCenterCoordinate(double topOffset = 200)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        var x = (screenWidth - WindowWidth) / 2;
        var y = topOffset >= double.MaxValue
            ? (screenHeight - WindowHeight) / 2
            : topOffset;

        return new(x, y);
    }

    #endregion
}