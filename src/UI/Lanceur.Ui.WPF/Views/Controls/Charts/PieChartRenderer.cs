using System.Globalization;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

/// <summary>
///     Draws a donut. The percentage is displayed next to the slices large enough to hold it, the legend
///     lists every category with its value and its percentage, so small slices remain readable.
/// </summary>
internal sealed class PieChartRenderer : IChartRenderer
{
    #region Fields

    /// <summary>
    ///     Below this share of the total, the percentage isn't displayed next to the slice: the labels of
    ///     small neighbouring slices would overlap.
    /// </summary>
    private const double MinShareWithLabel = 0.05;

    #endregion

    #region Methods

    public void Render(Plot plot, ChartDataViewModel chart, ChartTheme theme)
    {
        // An empty slice can't be drawn: its category is left out of the chart
        var points = chart.Points.Where(p => p.Y > 0).ToArray();
        var total = points.Sum(p => p.Y);

        var slices = points.Select((p, i) => {
                               var share = p.Y / total;
                               var percentage = share.ToString("P1", CultureInfo.CurrentCulture);
                               var value = p.Y.ToString("N0", CultureInfo.CurrentCulture);
                               return new PieSlice
                               {
                                   Value = p.Y,
                                   Label = share >= MinShareWithLabel ? percentage : string.Empty,
                                   LegendText = $"{p.Label}: {value} ({percentage})",
                                   FillColor = theme.Category(i),
                                   LabelFontColor = theme.Text
                               };
                           })
                           .ToList();

        var pie = plot.Add.Pie(slices);
        pie.DonutFraction = 0.5;
        pie.SliceLabelDistance = 1.2;
        pie.LineColor = Colors.Transparent;

        plot.HideAxesAndGrid();
        plot.ShowLegend(Edge.Right);
    }

    #endregion
}