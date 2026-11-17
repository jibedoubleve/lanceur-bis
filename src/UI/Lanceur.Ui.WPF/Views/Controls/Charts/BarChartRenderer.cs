using System.Globalization;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

internal sealed class BarChartRenderer : IChartRenderer
{
    #region Fields

    /// <summary>
    ///     Average width of a character relative to the font size, used to estimate label widths.
    /// </summary>
    private const double CharWidthRatio = 0.6;

    /// <summary>
    ///     Estimated width of the left axis (Y axis ticks and title), in pixels.
    ///     Only used to estimate the width available for the bars.
    /// </summary>
    private const double LeftAxisWidth = 80;

    /// <summary>
    ///     Above this number of bars, values are not displayed on top of the bars.
    /// </summary>
    private const int MaxBarsWithValueLabels = 15;

    /// <summary>
    ///     Date ticks currently displayed, kept to recompute their layout when the control is resized.
    /// </summary>
    private DateTicks? _dateTicks;

    #endregion

    #region Methods

    /// <summary>
    ///     Indexes of the bars starting a new period (month or year): the first bar and every
    ///     bar whose period differs from the previous one.
    /// </summary>
    private static int[] PeriodStarts(double[] sortedXPoints, Func<DateTime, int> period)
        => sortedXPoints.Select((_, i) => i)
                        .Where(i => i == 0
                                    || period(DateTime.FromOADate(sortedXPoints[i]))
                                    != period(DateTime.FromOADate(sortedXPoints[i - 1])))
                        .ToArray();

    /// <summary>
    ///     Smallest distance between the centres of two consecutive bars. The unit is a day
    ///     (about 30 between two months)
    /// </summary>
    private static double SmallestGap(double[] sortedXPoints)
        => sortedXPoints.Length <= 1
            ? 1
            : sortedXPoints.Zip(sortedXPoints.Skip(1), (a, b) => b - a)
                           .Where(d => d > 0)
                           .DefaultIfEmpty(1)
                           .Min();

    /// <summary>
    ///     Picks the most readable layout of the date ticks for the available width:
    ///     one horizontal label per bar, then labels rotated by 45°, then only one label every N bars.
    /// </summary>
    public bool Relayout(Plot plot, double width)
    {
        if (_dateTicks is null) { return false; }

        var (xPoints, xPointFormat) = _dateTicks;
        var axis = plot.Axes.Bottom;
        var fontSize = axis.TickLabelStyle.FontSize;

        var labels = xPoints.Select(x => DateTime.FromOADate(x).ToString(xPointFormat, CultureInfo.CurrentCulture))
                            .ToArray();
        var labelWidth = labels.Max(l => l.Length) * fontSize * CharWidthRatio;

        var dataWidth = Math.Max(1, width - LeftAxisWidth);
        var widthPerBar = dataWidth / xPoints.Length;

        // A rotated label needs roughly its line height (font size + spacing) of horizontal room
        var rotatedLabelWidth = fontSize * 1.5;

        var isHorizontal = labelWidth <= widthPerBar * 0.9;
        var step = isHorizontal || widthPerBar >= rotatedLabelWidth
            ? 1
            : (int)Math.Ceiling(rotatedLabelWidth / widthPerBar);

        axis.TickGenerator = new NumericManual(
            xPoints.Select((x, i) => (x, i))
                   .Where(t => t.i % step == 0)
                   .Select(t => new Tick(t.x, labels[t.i]))
                   .ToArray()
        );

        axis.TickLabelStyle.Rotation = isHorizontal ? 0 : -45;
        axis.TickLabelStyle.Alignment = isHorizontal ? Alignment.UpperCenter : Alignment.MiddleRight;

        // Rotated labels are taller: reserve room so they don't overlap the axis title
        axis.MinimumSize = isHorizontal ? 0 : (float)(labelWidth * Math.Sin(Math.PI / 4) + fontSize * 2);
        return true;
    }

    public void Render(Plot plot, ChartDataViewModel chart, ChartTheme theme)
    {
        // Sorted so the gap between two consecutive bars is meaningful
        var points = chart.Points.OrderBy(p => p.X).ToArray();
        var xPoints = points.Select(p => p.X).ToArray();
        var yPoints = points.Select(p => p.Y).ToArray();

        var bars = plot.Add.Bars(xPoints, yPoints);
        bars.Color = theme.Accent;
        bars.ValueLabelStyle.ForeColor = theme.SecondaryText;

        var showValues = points.Length <= MaxBarsWithValueLabels;
        foreach (var bar in bars.Bars)
        {
            bar.LineWidth = 0;
            if (showValues) { bar.Label = bar.Value.ToString("N0", CultureInfo.CurrentCulture); }
        }

        switch (chart.Descriptor.XAxisKind)
        {
            case AxisKind.Date:
                var gap = SmallestGap(xPoints);
                foreach (var bar in bars.Bars)
                {
                    // Bars are narrower than the gap so they don't touch each other
                    bar.Size = gap * 0.8;
                }

                if (gap >= 28)
                {
                    // Monthly or yearly data: one label per bar, laid out by Relayout
                    _dateTicks = new(xPoints, gap >= 365 ? "yyyy" : "MMMM yy");
                }
                else
                {
                    RenderDailyTicks(plot, xPoints, gap, theme);
                }

                break;

            case AxisKind.Numeric when points.Any(p => p.Label is not null):
            case AxisKind.Text:
                plot.Axes.Bottom.TickGenerator = new NumericManual(
                    points.Select(p => new Tick(p.X, p.Label ?? string.Empty)).ToArray()
                );
                break;
        }

        // Bars start at 0 with headroom beyond the highest values (room for the value labels)
        var top = Math.Max(0, yPoints.Max()) * 1.15;
        var bottom = Math.Min(0, yPoints.Min()) * 1.15;
        plot.Axes.AutoScale();
        plot.Axes.SetLimitsY(bottom, top > bottom ? top : bottom + 1);
    }

    /// <summary>
    ///     Daily data: one label per month (or per year on long ranges) and a separator between periods
    /// </summary>
    private static void RenderDailyTicks(Plot plot, double[] xPoints, double gap, ChartTheme theme)
    {
        var spanInDays = xPoints[^1] - xPoints[0];
        var (period, format, periodLength) = spanInDays > 2 * 365
            ? ((Func<DateTime, int>)(d => d.Year), "yyyy", 365d)
            : (d => d.Year * 12 + d.Month, "MMM yy", 30d);

        var starts = PeriodStarts(xPoints, period);

        // A truncated first period (e.g. the last days of 2019) leaves no room for its label:
        // keep only its separator
        var labelled = starts.Length > 1 && xPoints[starts[1]] - xPoints[0] < periodLength / 2
            ? starts.Skip(1)
            : starts;

        plot.Axes.Bottom.TickGenerator = new NumericManual(
            labelled.Select(i => new Tick(xPoints[i],
                        DateTime.FromOADate(xPoints[i]).ToString(format, CultureInfo.CurrentCulture)))
                    .ToArray()
        );

        // Separators sit between two bars, not on them (the first period needs none)
        foreach (var i in starts.Skip(1))
        {
            var line = plot.Add.VerticalLine(xPoints[i] - gap / 2);
            line.LinePattern = LinePattern.Dotted;
            line.LineWidth = 1;
            line.Color = theme.Grid;
        }
    }

    #endregion

    private sealed record DateTicks(double[] XPoints, string Format);
}
