namespace Lanceur.Core.Models;

public sealed record ChartPoint
{
    #region Properties

    public string? Label { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }

    #endregion
}