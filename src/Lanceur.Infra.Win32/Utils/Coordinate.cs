namespace Lanceur.Infra.Win32.Utils
{
    public class Coordinate
    {
        #region Constructors

        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion Constructors

        #region Properties

        public double X { get; set; }
        public double Y { get; set; }

        #endregion Properties
    }
}