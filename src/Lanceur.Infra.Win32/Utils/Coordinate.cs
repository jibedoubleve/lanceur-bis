namespace Lanceur.Infra.Win32.Utils;

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

    public bool IsEmpty => X == double.MaxValue && Y == double.MaxValue;
    public double X { get; }
    public double Y { get; }

    #endregion Properties
}