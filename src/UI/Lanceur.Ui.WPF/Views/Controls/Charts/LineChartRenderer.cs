using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

internal sealed class LineChartRenderer : IChartRenderer
{
    #region Fields

    /// <summary>
    ///     Above this number of points, the points are not marked on the line.
    /// </summary>
    private const int MaxPointsWithMarkers = 30;

    #endregion

    #region Methods

    public void Render(Plot plot, ChartDataViewModel chart, ChartTheme theme)
    {
        var points = chart.Points.OrderBy(p => p.X).ToArray();
        var xPoints = points.Select(p => p.X).ToArray();
        var yPoints = points.Select(p => p.Y).ToArray();

        var line = plot.Add.Scatter(xPoints, yPoints, theme.Accent);
        line.LineWidth = 2;
        line.MarkerSize = points.Length <= MaxPointsWithMarkers ? 6 : 0;

        switch (chart.Descriptor.XAxisKind)
        {
            case AxisKind.Date:
                plot.Axes.DateTimeTicksBottom();
                break;

            case AxisKind.Text:
                plot.Axes.Bottom.TickGenerator = new NumericManual(
                    points.Select(p => new Tick(p.X, p.Label ?? string.Empty)).ToArray()
                );
                break;
        }

        // The Y axis starts at 0 so the slope of the line isn't exaggerated
        var top = Math.Max(0, yPoints.Max()) * 1.05;
        var bottom = Math.Min(0, yPoints.Min()) * 1.05;
        plot.Axes.AutoScale();
        plot.Axes.SetLimitsY(bottom, top > bottom ? top : bottom + 1);
    }

    #endregion
}
