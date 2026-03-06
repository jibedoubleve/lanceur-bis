namespace Lanceur.Core.Models;

public sealed record AliasUsage
{
    #region Properties

    public int Count { get; set; }
    public required string Name { get; set; }
    public required int Year { get; set; }

    #endregion
}