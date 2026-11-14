namespace Lanceur.Core.Models;

public sealed record ChartDefinition
{
    #region Properties

    public required IEnumerable<ChartFilterDefinition> Filters { get; init; }
    public required Guid Id { get; init; }

    public required uint Order { get; init; }
    public required string Sql { get; init; }
    public required string Title { get; init; }
    public required string XKind { get; init; }
    public required string XLabel { get; init; }
    public required string YLabel { get; init; }

    #endregion
}