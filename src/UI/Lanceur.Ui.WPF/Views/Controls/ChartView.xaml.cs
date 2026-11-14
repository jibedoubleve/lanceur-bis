using System.Globalization;
using System.Windows;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using Wpf.Ui.Appearance;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class ChartView
{
    #region Fields

    public static readonly DependencyProperty ChartProperty =
        DependencyProperty.Register(
            nameof(Chart),
            typeof(ChartDataViewModel),
            typeof(ChartView),
            new PropertyMetadata(null, (d, e) => ((ChartView)d).Draw(e.NewValue as ChartDataViewModel)));

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

    #region Constructors

    public ChartView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Properties

    public ChartDataViewModel? Chart
    {
        get => (ChartDataViewModel?)GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    #endregion

    #region Methods

    private void ApplyTheme()
    {
        var plot = Plot.Plot;

        var text = ThemeColor("TextFillColorPrimaryBrush", Colors.Black);
        var secondaryText = ThemeColor("TextFillColorSecondaryBrush", Colors.Gray);
        var axis = ThemeColor("ControlStrongStrokeColorDefaultBrush", Colors.Gray);
        var grid = ThemeColor("DividerStrokeColorDefaultBrush", Colors.LightGray);

        // Transparent: the page background (Mica / Fluent background) shows through
        plot.FigureBackground.Color = Colors.Transparent;
        plot.DataBackground.Color = Colors.Transparent;

        plot.Axes.Color(secondaryText);
        plot.Axes.Title.Label.ForeColor = text;
        plot.Axes.Bottom.Label.ForeColor = text;
        plot.Axes.Left.Label.ForeColor = text;
        plot.Axes.Bottom.FrameLineStyle.Color = axis;
        plot.Axes.Left.FrameLineStyle.Color = axis;
        plot.Axes.Top.FrameLineStyle.Color = Colors.Transparent;
        plot.Axes.Right.FrameLineStyle.Color = Colors.Transparent;

        plot.Grid.MajorLineColor = grid;

        foreach (var bars in plot.GetPlottables<BarPlot>())
        {
            bars.Color = ThemeColor("AccentFillColorDefaultBrush", Color.FromHex("#0078D4"));
            bars.ValueLabelStyle.ForeColor = secondaryText;
        }

        foreach (var line in plot.GetPlottables<VerticalLine>())
        {
            line.Color = grid;
        }
    }

    private void Draw(ChartDataViewModel? chart)
    {
        var plot = Plot.Plot;
        plot.Clear();
        _dateTicks = null;
        ResetBottomTicks();

        if (chart is null || chart.Points.Count == 0)
        {
            ApplyTheme();
            Plot.Refresh();
            return;
        }

        // Sorted so the gap between two consecutive bars is meaningful
        var points = chart.Points.OrderBy(p => p.X).ToArray();
        var xPoints = points.Select(p => p.X).ToArray();
        var yPoints = points.Select(p => p.Y).ToArray();

        var bars = plot.Add.Bars(xPoints, yPoints);
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
                    // Monthly or yearly data: one label per bar
                    _dateTicks = new DateTicks(xPoints, gap >= 365 ? "yyyy" : "MMMM yy");
                    LayoutDateTicks();
                }
                else
                {
                    // Daily data: one label per month (or per year on long ranges) and a separator between periods
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
                    }
                }

                break;

            case AxisKind.Numeric when points.Any(p => p.Label is not null):
                plot.Axes.Bottom.TickGenerator = new NumericManual(
                    points.Select(p => new Tick(p.X, p.Label ?? string.Empty)).ToArray()
                );
                break;
            
            case AxisKind.Text:
                plot.Axes.Bottom.TickGenerator = new NumericManual(
                    points.Select(p => new Tick(p.X, p.Label ?? string.Empty)).ToArray()
                );
                break;
        }

        plot.Title(chart.Descriptor.Title);
        plot.XLabel(chart.Descriptor.XLabel);
        plot.YLabel(chart.Descriptor.YLabel);

        // Bars start at 0 with headroom above the highest value (room for the value labels)
        plot.Axes.AutoScale();
        plot.Axes.SetLimitsY(0, yPoints.Max() * 1.15);

        ApplyTheme();
        Plot.Refresh();
        return;

        // Smallest distance between the centres of two consecutive bars. The unit is a day
        // (about 30 between two months)
        static double SmallestGap(double[] sortedXPoints)
            => sortedXPoints.Length <= 1
                ? 1
                : sortedXPoints.Zip(sortedXPoints.Skip(1), (a, b) => b - a)
                               .Where(d => d > 0)
                               .DefaultIfEmpty(1).Min();


        // Indexes of the bars starting a new period (month or year): the first bar and every
        // bar whose period differs from the previous one.
        static int[] PeriodStarts(double[] sortedXPoints, Func<DateTime, int> period)
            => sortedXPoints.Select((_, i) => i)
                            .Where(i => i == 0
                                        || period(DateTime.FromOADate(sortedXPoints[i]))
                                        != period(DateTime.FromOADate(sortedXPoints[i - 1])))
                            .ToArray();
    }

    /// <summary>
    ///     Picks the most readable layout of the date ticks for the available width:
    ///     one horizontal label per bar, then labels rotated by 45°, then only one label every N bars.
    /// </summary>
    private void LayoutDateTicks()
    {
        if (_dateTicks is null) { return; }

        var (xPoints, xPointFormat) = _dateTicks;
        var axis = Plot.Plot.Axes.Bottom;
        var fontSize = axis.TickLabelStyle.FontSize;

        var labels = xPoints.Select(x => DateTime.FromOADate(x).ToString(xPointFormat, CultureInfo.CurrentCulture))
                            .ToArray();
        var labelWidth = labels.Max(l => l.Length) * fontSize * CharWidthRatio;

        var dataWidth = Math.Max(1, Plot.ActualWidth - LeftAxisWidth);
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
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Changed += OnThemeChanged;
        Draw(Chart);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_dateTicks is null || !e.WidthChanged) { return; }

        LayoutDateTicks();
        Plot.Refresh();
    }

    private void OnThemeChanged(ApplicationTheme theme, System.Windows.Media.Color accent) => Draw(Chart);

    private void OnUnloaded(object sender, RoutedEventArgs e) => ApplicationThemeManager.Changed -= OnThemeChanged;

    /// <summary>
    ///     <c>Plot.Clear()</c> removes the plottables but keeps the axis configuration:
    ///     restore the default ticks so a previous chart doesn't leak into the next one.
    /// </summary>
    private void ResetBottomTicks()
    {
        var axis = Plot.Plot.Axes.Bottom;
        axis.TickGenerator = new NumericAutomatic();
        axis.TickLabelStyle.Rotation = 0;
        axis.TickLabelStyle.Alignment = Alignment.UpperCenter;
        axis.MinimumSize = 0;
    }


    /// <summary>
    ///     Reads a Fluent theme brush from the WPF-UI resources so the chart follows
    ///     the same palette as the rest of the application (light, dark and accent).
    /// </summary>
    private Color ThemeColor(string resourceKey, Color fallback)
        => TryFindResource(resourceKey) is SolidColorBrush brush
            ? new Color(brush.Color.R,
                brush.Color.G,
                brush.Color.B,
                brush.Color.A)
            : fallback;

    #endregion

    private sealed record DateTicks(double[] XPoints, string Format);
}