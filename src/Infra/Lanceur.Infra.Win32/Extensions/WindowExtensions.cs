using System.Windows;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Extensions;

public static class WindowExtensions
{
    #region Fields

    public const double DefaultTopOffset = 200;

    #endregion

    #region Methods

    public static Coordinate GetCenterCoordinate(this Window win, double topOffset = DefaultTopOffset)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        var x = (screenWidth - win.Width) / 2;
        var y = topOffset >= double.MaxValue
            ? (screenHeight - win.Height) / 2
            : topOffset;

        return new(x, y);
    }


    public static bool IsInScreen(this Window window)
    {
        // Obtain the dimensions and position of the window
        var winRect = new Rect(
            window.Left,
            window.Top,
            window.ActualWidth,
            window.ActualHeight
        );

        // Check if the window is completely or partially off the screen
        return SystemParameters.WorkArea.Contains(winRect);
    }

    public static void SetDefaultPosition(this Window window)
    {
        var coordinate = window.GetCenterCoordinate();
        window.SetPosition(coordinate);
    }

    public static void SetPosition(this Window win, Coordinate coordinate)
    {
        win.Left = coordinate.X;
        win.Top = coordinate.Y;
    }

    #endregion
}