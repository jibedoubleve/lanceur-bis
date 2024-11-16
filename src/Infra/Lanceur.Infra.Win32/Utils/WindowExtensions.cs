using System.Windows;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Utils;

public static class WindowExtensions
{
    #region Fields

    public const double DefaultTopOffset = 200;

    #endregion Fields

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

    public static Coordinate SetDefaultPosition(this Window win)
    {
        var coordinate = win.GetCenterCoordinate(DefaultTopOffset);
        win.SetPosition(coordinate);
        return coordinate;
    }

    public static void SetPosition(this Window win, Coordinate coordinate)
    {
        win.Left = coordinate.X;
        win.Top = coordinate.Y;
    }


    public static bool IsOutOfScreen(this Window win)
    {
        // Obtain the dimensions of the primary screen
        var workArea = SystemParameters.WorkArea;

        // Obtain the dimensions and position of the window
        var winLeft = win.Left;
        var winTop = win.Top;
        var winWidth = winLeft + win.Width;
        var winHeight = winTop + win.Height;

        // Check if the window is completely or partially off the screen
        return winLeft < workArea.Left ||
               winTop < workArea.Top ||
               winWidth > workArea.Right ||
               winHeight > workArea.Bottom;
    }

    #endregion Methods
}