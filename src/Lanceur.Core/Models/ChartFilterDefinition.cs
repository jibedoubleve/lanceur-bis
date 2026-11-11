namespace Lanceur.Core.Models;

public sealed record ChartFilterDefinition
{
    #region Properties

    public required string ParamName { get; init; }
    public required string Sql { get; init; }
    public required string Title { get; init; }

    #endregion
}