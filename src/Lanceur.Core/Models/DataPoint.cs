namespace Lanceur.Core.Models;

public record DataPoint<Tx, Ty>
{
    #region Properties

    public required Tx X { get; init; }
    public required Ty Y { get; init; }

    #endregion
}