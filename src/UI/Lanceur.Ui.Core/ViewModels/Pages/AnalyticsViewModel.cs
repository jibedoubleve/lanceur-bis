using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class AnalyticsViewModel : ObservableObject
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    private CancellationTokenSource _cancellationCacheTokenSource = new();
    [ObservableProperty] private PlotType _currentPlotType;
    [ObservableProperty] private bool _isMonthlyVisible;
    private readonly ILogger<AnalyticsViewModel> _logger;
    private readonly IMemoryCache _memoryCache;
    [ObservableProperty] private ObservableCollection<string> _years = [];
    private const string CacheKey = $"analytics_{nameof(AnalyticsViewModel)}";

    private const string SelectAll = "All";

    #endregion

    #region Constructors

    public AnalyticsViewModel(ILogger<AnalyticsViewModel> logger, IAliasRepository aliasRepository, IMemoryCache memoryCache)
    {
        _logger = logger;
        _aliasRepository = aliasRepository;
        _memoryCache = memoryCache;
    }

    #endregion

    #region Properties

    private PlotContext? LastPlotContext { get; set; }

    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshDailyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshMonthlyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshUsageByDayOfWeekPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshUsageByHourPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>>? OnRefreshYearlyPlot { get; set; }

    #endregion

    #region Methods

    private ObservableCollection<string> GetYears(IEnumerable<DataPoint<DateTime, double>> points)
    {
        var list = points.Select(e => e.X.Year.ToString())
                         .Distinct()
                         .ToList();
        list.Insert(0, SelectAll);
        return new(list);
    }

    private void InvalidateCache(IEnumerable<DataPoint<DateTime, double>> points)
    {
        _cancellationCacheTokenSource.Cancel();
        _cancellationCacheTokenSource = new(1.Minutes());
        _memoryCache.Set(CacheKey, points, new CancellationChangeToken(_cancellationCacheTokenSource.Token));
    }

    [RelayCommand]
    private async Task OnRefreshDailyHistory()
    {
        var points = await Task.Run(() => _aliasRepository.GetUsage(Per.Day));
        points = points.ToList();
        InvalidateCache(points);
        RedrawPlot(OnRefreshDailyPlot, points, PlotType.DailyHistory);
    }

    [RelayCommand]
    private async Task OnRefreshMonthlyHistory()
    {
        var points = await Task.Run(() => _aliasRepository.GetUsage(Per.Month));
        points = points.ToList();
        InvalidateCache(points);

        IsMonthlyVisible = points.Any();
        if (IsMonthlyVisible)
            RedrawPlot(OnRefreshMonthlyPlot, points, PlotType.MonthlyHistory);
        else // To be here means there's nothing to show, fallback is daily history...
            await OnRefreshDailyHistory();
    }

    [RelayCommand]
    private async Task OnRefreshUsageByDayOfWeek()
    {
        var points = await Task.Run(() => _aliasRepository.GetUsage(Per.DayOfWeek));
        points = points.ToList();
        InvalidateCache(points);
        RedrawPlot(
            OnRefreshUsageByDayOfWeekPlot,
            points,
            PlotType.UsageByDayOfWeek,
            e => (double)e.DayOfWeek
        );
    }

    [RelayCommand]
    private async Task OnRefreshUsageByHourOfDay()
    {
        var points = await Task.Run(() => _aliasRepository.GetUsage(Per.HourOfDay));
        points = points.ToList();
        InvalidateCache(points);
        RedrawPlot(
            OnRefreshUsageByHourPlot,
            points,
            PlotType.UsageByHourOfDay,
            e => e.TimeOfDay.TotalHours
        );
    }

    [RelayCommand]
    private async Task OnRefreshYearlyHistory()
    {
        var points = await Task.Run(() => _aliasRepository.GetUsage(Per.Year));
        points = points.ToList();
        InvalidateCache(points);

        IsMonthlyVisible = points.Any();
        if (IsMonthlyVisible)
            RedrawPlot(OnRefreshYearlyPlot, points, PlotType.YearlyHistory);
        else // To be here means there's nothing to show, fallback is daily history...
            await OnRefreshDailyHistory();
    }

    [RelayCommand]
    private void OnSelectYear(string year)
    {
        _logger.LogDebug("Display history for year {Year}", year);

        if (LastPlotContext?.IsTrendPlot() ?? false)
        {
            // Trends charts means we have to go back to the database
            _memoryCache.Remove(CacheKey);
            RedrawLastTrendPlot(year);
            return;
        }

        var p = _memoryCache.Get<IEnumerable<DataPoint<DateTime, double>>>(CacheKey) ?? [];
        var points = year == SelectAll ? p : p.Where(e => e.X.Year.ToString() == year);

        RedrawPlot(LastPlotContext, points);
    }

    private void RedrawLastTrendPlot(string yearStr)
    {
        if (LastPlotContext is null) return;

        int? year = int.TryParse(yearStr, out var yearValue) ? yearValue : null;
        var per =  LastPlotContext.PlotType switch
        {
            PlotType.UsageByHourOfDay =>  Per.HourOfDay,
            PlotType.UsageByDayOfWeek => Per.DayOfWeek,
            _                         => throw new ArgumentOutOfRangeException($"Plot '{LastPlotContext.PlotType}' is not supported for a trend plot refresh.")
        };
        var points = _aliasRepository.GetUsage(per, year);
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

        #region Methods

        public bool IsTrendPlot()
        {
            PlotType[] trends = [PlotType.UsageByDayOfWeek, PlotType.UsageByHourOfDay];

            return trends.Contains(PlotType);
        }

        #endregion
    }
}