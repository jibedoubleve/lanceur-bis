using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class AnalyticsViewModel : ObservableObject
{
    #region Fields

    private IEnumerable<DataPoint<DateTime, double>> _cachedDataPoints = [];

    [ObservableProperty] private PlotType _currentPlotType;
    private readonly IDbRepository _dbRepository;
    [ObservableProperty] private bool _isMonthlyVisible;
    [ObservableProperty] private bool _isYearVisible;
    private readonly ILogger<AnalyticsViewModel> _logger;
    [ObservableProperty] private ObservableCollection<string> _years = [];

    #endregion

    #region Constructors

    public AnalyticsViewModel(ILogger<AnalyticsViewModel> logger, IDbRepository dbRepository)
    {
        _logger = logger;
        _dbRepository = dbRepository;
    }

    #endregion

    #region Properties

    private PlotContext? LastPlotContext { get; set; }

    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshDailyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshMonthlyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshUsageByDayOfWeekPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshUsageByHourPlot { get; set; }

    #endregion

    #region Methods

    private void Cache(IEnumerable<DataPoint<DateTime, double>> points) => _cachedDataPoints = points;

    private ObservableCollection<string> GetYears(IEnumerable<DataPoint<DateTime, double>> points)
    {
        var list = points.Select(e => e.X.Year.ToString())
                         .Distinct()
                         .ToList();
        list.Insert(0, "All");
        return new(list);
    }

    [RelayCommand]
    private async Task OnRefreshDailyHistory()
    {
        IsYearVisible = true;
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.Day));
        points = points.ToList();
        Cache(points);
        RedrawPlot(OnRefreshDailyPlot, points, PlotType.DailyHistory);
    }

    [RelayCommand]
    private async Task OnRefreshMonthlyHistory()
    {
        IsYearVisible = true;
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.Month));
        points = points.ToList();
        Cache(points);

        IsMonthlyVisible = points.Any();
        if (IsMonthlyVisible)
            RedrawPlot(OnRefreshMonthlyPlot, points, PlotType.MonthlyHistory);
        else // To be here means there's nothing to show, fallback is daily history...
            await OnRefreshDailyHistory();
    }

    [RelayCommand]
    private async Task OnRefreshUsageByDayOfWeek()
    {
        IsYearVisible = false;
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.DayOfWeek));
        points = points.ToList();
        Cache(points);
        RedrawPlot(OnRefreshUsageByDayOfWeekPlot, points, PlotType.UsageByDayOfWeek, e => (double)e.DayOfWeek);
    }

    [RelayCommand]
    private async Task OnRefreshUsageByHourOfDay()
    {
        IsYearVisible = false;
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.HourOfDay));
        points = points.ToList();
        Cache(points);
        RedrawPlot(OnRefreshUsageByHourPlot, points, PlotType.UsageByHourOfDay, e => e.TimeOfDay.TotalHours);
    }

    [RelayCommand]
    private void OnSelectYear(string year)
    {
        _logger.LogTrace("Display history for year {Year}", year);
        var points = int.TryParse(year, out var y)
            ? _cachedDataPoints.Where(p => p.X.Year == y)
            : _cachedDataPoints;

        RedrawPlot(LastPlotContext, points);
    }

    private void RedrawPlot(PlotContext? plotContext, IEnumerable<DataPoint<DateTime, double>> dataPoints, [CallerMemberName] string? caller = null)
    {
        if (plotContext is null) return;

        RedrawPlot(
            plotContext.RedrawPlotAction,
            dataPoints,
            plotContext.PlotType,
            plotContext.DateTimeToDoubleConverter,
            caller
        );
    }

    private void RedrawPlot(
        Action<IEnumerable<double>, IEnumerable<double>>? redrawPlotAction,
        IEnumerable<DataPoint<DateTime, double>> dataPoints,
        PlotType plotType,
        Func<DateTime, double>? dateTimeToDoubleConverter = null,
        [CallerMemberName] string? caller = null
    )
    {
        dateTimeToDoubleConverter ??= r => r.ToOADate();
        dataPoints = dataPoints.ToList();

        if (LastPlotContext is null) Years = GetYears(dataPoints);
        LastPlotContext = new() { DateTimeToDoubleConverter = dateTimeToDoubleConverter, PlotType = plotType, RedrawPlotAction = redrawPlotAction };

        _logger.LogDebug("[ViewModel] Refreshing plot {Caller}", caller);
        var points = dataPoints.ToArray();
        var x = points.Select(p => dateTimeToDoubleConverter(p.X)).ToArray();
        var y = points.Select(p => p.Y).ToArray();
        CurrentPlotType = plotType;
        redrawPlotAction?.Invoke(x, y);
    }

    #endregion

    private record PlotContext
    {
        #region Properties

        /// <summary>
        ///     A function used to convert the X value of the points from DateTime to double.
        ///     This is useful for transforming time-based data into a numerical format suitable for plotting.
        /// </summary>
        public Func<DateTime, double>? DateTimeToDoubleConverter { get; init; }

        /// <summary>
        ///     The type of the plot to be displayed. This property determines the visual style and characteristics of the plot.
        /// </summary>
        public PlotType PlotType { get; init; }

        /// <summary>
        ///     An action that is executed to refresh the plot with the current points to display.
        ///     This action takes two collections of doubles, representing the X and Y coordinates of the points.
        /// </summary>
        public Action<IEnumerable<double>, IEnumerable<double>>? RedrawPlotAction { get; init; }

        #endregion
    }
}