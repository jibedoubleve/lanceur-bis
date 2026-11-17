using System.Globalization;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

/// <summary>
///     Draws a grid where each column is a serie and each row a label (e.g. the days of the week and
///     the hours of the day). The <c>X</c> of the points is ignored: with a text axis, it is the index of
///     the point in the whole result, not the index of the row.
/// </summary>
internal sealed class HeatmapChartRenderer : IChartRenderer
{
    #region Fields

    /// <summary>
    ///     Above this number of rows, only one row label out of N is displayed.
    /// </summary>
    private const int MaxRowLabels = 12;

    #endregion

    #region Methods

    public void Render(Plot plot, ChartDataViewModel chart, ChartTheme theme)
    {
        // Rows and columns keep the order of the query
        var columns = chart.Points.Select(p => p.Serie ?? string.Empty).Distinct().ToArray();
        var rows = chart.Points.Select(RowOf).Distinct().ToArray();

        var columnIndexes = columns.Select((c, i) => (c, i)).ToDictionary(t => t.c, t => t.i);
        var rowIndexes = rows.Select((r, i) => (r, i)).ToDictionary(t => t.r, t => t.i);

        var intensities = new double[rows.Length, columns.Length];
        foreach (var point in chart.Points)
        {
            intensities[rowIndexes[RowOf(point)], columnIndexes[point.Serie ?? string.Empty]] = point.Y;
        }

        // ScottPlot draws the first row at the top and centres each cell on integer coordinates
        var heatmap = plot.Add.Heatmap(intensities);
        heatmap.Colormap = new AccentColormap(theme.Accent);

        var colorBar = plot.Add.ColorBar(heatmap);
        colorBar.Label = chart.Descriptor.YLabel ?? string.Empty;
        colorBar.LabelStyle.ForeColor = theme.Text;
        colorBar.Axis.TickLabelStyle.ForeColor = theme.SecondaryText;
        colorBar.Axis.FrameLineStyle.Color = theme.Axis;

        plot.Axes.Bottom.TickGenerator = new NumericManual(
            [.. columns.Select((c, i) => new Tick(i, c))]
        );

        var step = (int)Math.Ceiling(rows.Length / (double)MaxRowLabels);
        plot.Axes.Left.TickGenerator = new NumericManual(
            [
                .. rows.Select((r, i) => (r, i))
                       .Where(t => t.i % step == 0)
                       .Select(t => new Tick(rows.Length - 1 - t.i, t.r))
            ]
        );

        // The X axis shows the series, the Y axis the labels and the intensity is described by the colour bar
        plot.XLabel(chart.Descriptor.SeriesLabel ?? string.Empty);
        plot.YLabel(chart.Descriptor.XLabel ?? string.Empty);
        plot.HideGrid();
        plot.Axes.SetLimits(-0.5, columns.Length - 0.5, -0.5, rows.Length - 0.5);
        return;

        static string RowOf(ChartPoint point) => point.Label ?? point.X.ToString(CultureInfo.CurrentCulture);
    }

    #endregion

    /// <summary>
    ///     Shades of the accent colour, from almost transparent (lowest intensity) to opaque (highest).
    /// </summary>
    private sealed class AccentColormap(Color accent) : IColormap
    {
        #region Fields

        private const double MinOpacity = 0.06;

        #endregion

        #region Properties

        public string Name => "Accent";

        #endregion

        #region Methods

        public Color GetColor(double normalizedIntensity)
            => accent.WithOpacity(MinOpacity + (1 - MinOpacity) * Math.Clamp(normalizedIntensity, 0, 1));

        #endregion
    }
}
