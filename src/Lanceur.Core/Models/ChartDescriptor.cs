namespace Lanceur.Core.Models;

/// <summary>
///     Contains a description of a chart
/// </summary>
public sealed record ChartDescriptor
{
    #region Properties

    public required IReadOnlyCollection<ChartFilter> Filters { get; init; }
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required AxisKind XAxisKind { get; init; }
    public required string XLabel { get; init; }
    public required string YLabel { get; init; }

    #endregion
}

public enum AxisKind { Numeric, Date, Text }