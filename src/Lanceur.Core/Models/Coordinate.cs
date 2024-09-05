namespace Lanceur.Core.Models;

public record Coordinate(double X, double Y)
{
    #region Properties

    public bool IsEmpty => X == double.MaxValue && Y == double.MaxValue;

    #endregion

    #region Methods

    public override string ToString() => $"({X}, {Y})";

    #endregion
}