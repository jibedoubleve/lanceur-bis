using Lanceur.Core.Charts;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;
using ScottPlot;
using Wpf.Ui.Appearance;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class AnalyticsView : IDisposable
{
    #region Fields

    private readonly ILogger<AnalyticsView> _logger;

    #endregion

    #region Constructors

    public AnalyticsView(AnalyticsViewModel? viewModel, ILogger<AnalyticsView> logger)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _logger = logger;

        viewModel.OnRefreshDailyPlot = (x, y) => RefreshScatter(x, y, "Daily history");
        viewModel.OnRefreshMonthlyPlot = (x, y) => RefreshScatter(x, y, "Monthly history");
        viewModel.OnRefreshUsageByHourPlot = (x, y) => RefreshBar(x, y, DoubleConverter.ToTimeString, "Usage by hour of day", "Hours of day");
        viewModel.OnRefreshUsageByDayOfWeekPlot = (x, y) => RefreshBar(x, y, DoubleConverter.ToDayOfWeek, "Usage by day of week", "Day of week");

        DataContext = viewModel;
        ApplicationThemeManager.Changed += OnThemeChanged;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private ThemeColor CurrentTheme => ApplicationThemeManager.IsMatchedDark() ? ThemeColor.Dark : ThemeColor.Light;

    #endregion

    #region Methods

    private void OnThemeChanged(ApplicationTheme _, System.Windows.Media.Color __) => SetTheme();

    private void RefreshBar(IEnumerable<double> xPoint, IEnumerable<double> yPoint, Func<double, string> convertBarName, string plotTitle, string bottomAxesTitle)
    {
        var x = xPoint.ToArray();
        var y = yPoint.ToArray();

        _logger.LogDebug("[View] Refreshing BAR plot (Points: {Points})", x.Length);

        if (!x.Any() || !y.Any()) return;

        HistoryPlot.Reset();
        HistoryPlot.Plot.Clear();

        var positions = Enumerable.Range(0, y.Length)
                                  .Select(e => (double)e)
                                  .ToArray();

        var barPlot = HistoryPlot.Plot.Add.Bars(positions, y);
        barPlot.ValueLabelStyle.ForeColor = CurrentTheme.LegendFontColor;
        for (var i = 0; i < positions.Length; i++)
        {
            var b = barPlot.Bars.ElementAt(i);
            b.Label = convertBarName(i);
        }

        SetTheme();
        HistoryPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        HistoryPlot.Plot.Axes.Title.Label.Text = plotTitle;
        HistoryPlot.Plot.Axes.Bottom.Label.Text = bottomAxesTitle;
        HistoryPlot.Plot.Axes.Left.Label.Text = "Usage";
        HistoryPlot.Refresh();
    }

    private void RefreshScatter(IEnumerable<double> xPoint, IEnumerable<double> yPoint, string plotTitle)
    {
        var x = xPoint.ToArray();
        var y = yPoint.ToArray();

        _logger.LogDebug("[View] Refreshing SCATTER plot (Points: {Points})", x.Length);

        if (!x.Any() || !y.Any()) return;

        HistoryPlot.Plot.Clear();

        var plot = HistoryPlot.Plot.Add.Scatter(x, y);
        plot.ConnectStyle = ConnectStyle.StepHorizontal;

        HistoryPlot.Plot.Axes.Title.Label.Text = plotTitle;

        // Styling
        HistoryPlot.Plot.Axes.DateTimeTicksBottom();
        SetTheme();

        // Dates
        HistoryPlot.Plot.Axes.Bottom.Label.Text = "Date";
        HistoryPlot.Plot.Axes.Bottom.Max = x.Max();
        HistoryPlot.Plot.Axes.Bottom.Min = x.Min();

        // Usages
        var uMax = y.Max();
        HistoryPlot.Plot.Axes.Left.Label.Text = "Usage";
        HistoryPlot.Plot.Axes.Left.Max = uMax + uMax * .1;
        HistoryPlot.Plot.Axes.Left.Min = 0;
        HistoryPlot.Refresh();
    }

    private void SetTheme()
    {
        var plot = HistoryPlot.Plot;

        // set the color palette used when coloring new items added to the plot
        plot.Add.Palette = CurrentTheme.Palette;

        // change figure colors
        plot.FigureBackground.Color = CurrentTheme.FigureBackground;
        plot.DataBackground.Color = CurrentTheme.DataBackground;

        // change axis and grid colors
        plot.Axes.Color(CurrentTheme.Axes);
        plot.Grid.MajorLineColor = CurrentTheme.MajorLineColor;

        // change legend colors
        plot.Legend.BackgroundColor = CurrentTheme.LegendBackgroundColor;
        plot.Legend.FontColor = CurrentTheme.LegendFontColor;
        plot.Legend.OutlineColor = CurrentTheme.LegendOutlineColor;

        HistoryPlot.Refresh();
    }

    public void Dispose()
    {
        _logger.LogDebug("Dispose view {ViewName}", nameof(AnalyticsView));
        ApplicationThemeManager.Changed -= OnThemeChanged;
    }

    #endregion

    private class ThemeColor
    {
        #region Fields

        private readonly bool _isDark;

        #endregion

        #region Constructors

        private ThemeColor(bool isDark) => _isDark = isDark;

        #endregion

        #region Properties

        public Color Axes => _isDark ? Color.FromHex("#d7d7d7") : Color.FromHex("#616161");
        public static ThemeColor Dark => new(true);
        public Color DataBackground => _isDark ? Color.FromHex("#1f1f1f") : Color.FromHex("#010101000");
        public Color FigureBackground => _isDark ? Color.FromHex("#181818") : Color.FromHex("#FFFFFF");
        public Color LegendBackgroundColor => _isDark ? Color.FromHex("#404040") : Color.FromHex("#FFFFFF");
        public Color LegendFontColor => _isDark ? Color.FromHex("#d7d7d7") : Color.FromHex("#ff0000");
        public Color LegendOutlineColor => _isDark ? Color.FromHex("#d7d7d7") : Color.FromHex("#000000");
        public static ThemeColor Light => new(false);
        public Color MajorLineColor => _isDark ? Color.FromHex("#404040") : Color.FromHex("#d7d7d7");

        public IPalette Palette =>  _isDark ? new ScottPlot.Palettes.Penumbra() : new ScottPlot.Palettes.Nord();

        #endregion
    }
}