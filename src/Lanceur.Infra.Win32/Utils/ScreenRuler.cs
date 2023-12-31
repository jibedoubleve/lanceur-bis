using System.Windows;

namespace Lanceur.Infra.Win32.Utils
{
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

        public static Coordinate GetCenterCoordinate(double topOffset = double.MaxValue)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            var x = (screenWidth - WindowWidth) / 2;
            var y = topOffset == double.MaxValue
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

        #endregion Methods
    }
}