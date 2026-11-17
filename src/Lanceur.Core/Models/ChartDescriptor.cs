namespace Lanceur.Core.Models;

/// <summary>
///     Contains a description of a chart
/// </summary>
public sealed record ChartDescriptor
{
    #region Properties

    public required ChartType ChartType { get; init; }
    public required IReadOnlyCollection<ChartFilter> Filters { get; init; }
    public required Guid Id { get; init; }
    public string? SeriesLabel { get; init; }
    public required string Title { get; init; }
    public required AxisKind XAxisKind { get; init; }
    public string? XLabel { get; init; }
    public string? YLabel { get; init; }

    #endregion
}

public enum AxisKind { Numeric, Date, Text }