using System.Windows;
using Lanceur.Core.Charts;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.Palettes;
using Wpf.Ui.Appearance;
using Color = System.Windows.Media.Color;

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

    /// <remarks>
    ///     Click on the menu should reset the year
    /// </remarks>
    /// >
    private void OnClickMenu(object sender, RoutedEventArgs e) { CbYears.SelectedIndex = 0; }

    private void OnThemeChanged(ApplicationTheme _, Color __) => SetTheme();

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
        barPlot.Color = CurrentTheme.Palette.Colors[0];
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

        // Styling
        HistoryPlot.Plot.Axes.DateTimeTicksBottom();
        SetTheme();

        // Manage data

        HistoryPlot.Plot.Clear();

        var plot = HistoryPlot.Plot.Add.Scatter(x, y);
        plot.ConnectStyle = ConnectStyle.StepHorizontal;

        HistoryPlot.Plot.Axes.Title.Label.Text = plotTitle;

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

        public ScottPlot.Color Axes => _isDark ? ScottPlot.Color.FromHex("#d7d7d7") : ScottPlot.Color.FromHex("#616161");
        public static ThemeColor Dark => new(true);
        public ScottPlot.Color DataBackground => _isDark ? ScottPlot.Color.FromHex("#1f1f1f") : ScottPlot.Color.FromHex("#010101000");
        public ScottPlot.Color FigureBackground => _isDark ? ScottPlot.Color.FromHex("#181818") : ScottPlot.Color.FromHex("#FFFFFF");
        public ScottPlot.Color LegendBackgroundColor => _isDark ? ScottPlot.Color.FromHex("#404040") : ScottPlot.Color.FromHex("#FFFFFF");
        public ScottPlot.Color LegendFontColor => _isDark ? ScottPlot.Color.FromHex("#d7d7d7") : ScottPlot.Color.FromHex("#ff0000");
        public ScottPlot.Color LegendOutlineColor => _isDark ? ScottPlot.Color.FromHex("#d7d7d7") : ScottPlot.Color.FromHex("#000000");
        public static ThemeColor Light => new(false);
        public ScottPlot.Color MajorLineColor => _isDark ? ScottPlot.Color.FromHex("#404040") : ScottPlot.Color.FromHex("#d7d7d7");

        public IPalette Palette =>  _isDark ? new Penumbra() : new Nord();

        #endregion
    }
}