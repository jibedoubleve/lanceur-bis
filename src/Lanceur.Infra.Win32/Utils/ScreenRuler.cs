using System.Windows;

namespace Lanceur.Infra.Win32.Utils;

public static class ScreenRuler
{
    #region Fields

    public const double DefaultTopOffset = 200;

    #endregion Fields

    #region Properties

    private static double WindowHeight => Application.Current.MainWindow!.Height;

    private static double WindowWidth => Application.Current.MainWindow!.Width;

    #endregion Properties

    #region Methods

    private static Coordinate GetCenterCoordinate(double topOffset)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        var x = (screenWidth - WindowWidth) / 2;
        var y = topOffset >= double.MaxValue
            ? (screenHeight - WindowHeight) / 2
            : topOffset;

        return new(x, y);
    }

    public static Coordinate SetDefaultPosition()
    {
        var coordinate = GetCenterCoordinate(DefaultTopOffset);
        SetWindowPosition(coordinate);
        return coordinate;
    }

    public static void SetWindowPosition(Coordinate coordinate)
    {
        var win = Application.Current.MainWindow;
        if (win is null) return;

        win.Left = coordinate.X;
        win.Top = coordinate.Y;
    }


    public static bool IsWindowOutOfScreen()
    {
        var win = Application.Current.MainWindow;
        if (win is null) return true;

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