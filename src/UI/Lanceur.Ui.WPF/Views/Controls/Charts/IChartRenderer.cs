using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

/// <summary>
///     Draws one type of chart. A new renderer is created each time a chart is drawn, so it can keep
///     the state it needs to adapt its layout when the control is resized.
/// </summary>
internal interface IChartRenderer
{
    #region Methods

    /// <summary>
    ///     Adapts the layout to the width of the control.
    /// </summary>
    /// <returns><c>true</c> when the layout changed and the plot must be refreshed.</returns>
    bool Relayout(Plot plot, double width) => false;

    /// <summary>
    ///     Adds the plottables of the chart. The plot is empty, its title and axis labels are already set.
    /// </summary>
    void Render(Plot plot, ChartDataViewModel chart, ChartTheme theme);

    #endregion
}

internal static class ChartRenderer
{
    #region Methods

    public static IChartRenderer Create(ChartType chartType)
        => chartType switch
        {
            ChartType.Bar     => new BarChartRenderer(),
            ChartType.Line    => new LineChartRenderer(),
            ChartType.Pie     => new PieChartRenderer(),
            ChartType.Heatmap => new HeatmapChartRenderer(),
            _                 => throw new NotSupportedException($"Unsupported chart type: {chartType}")
        };

    #endregion
}
