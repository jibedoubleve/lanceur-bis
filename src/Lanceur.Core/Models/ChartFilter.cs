namespace Lanceur.Core.Models;

/// <summary>
///     A filter of a chart: a finite list of options whose selected value is bound to a parameter of the chart query.
/// </summary>
public sealed record ChartFilter
{
    #region Properties

    /// <summary>
    ///     Values the user can pick from, keyed by the label displayed in the UI.
    ///     The selected value is bound as-is to <see cref="Parameter" />.
    /// </summary>
    public required IDictionary<string, object?> Options { get; init; }

    /// <summary>
    ///     Name of the SQL parameter this filter binds, without the <c>@</c> prefix.
    /// </summary>
    public required string Parameter { get; init; }

    /// <summary>
    ///     Label of the filter in the UI.
    /// </summary>
    public required string Title { get; init; }

    #endregion
}