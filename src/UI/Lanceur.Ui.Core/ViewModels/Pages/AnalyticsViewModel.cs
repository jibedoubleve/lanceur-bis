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

    [ObservableProperty] private PlotType _currentPlotType;
    private readonly IDbRepository _dbRepository;

    private readonly ILogger<AnalyticsViewModel> _logger;

    #endregion

    #region Constructors

    public AnalyticsViewModel(ILogger<AnalyticsViewModel> logger, IDbRepository dbRepository)
    {
        _logger = logger;
        _dbRepository = dbRepository;
    }

    #endregion

    #region Properties

    public Action<IEnumerable<double>, IEnumerable<double>> OnRefreshDailyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>> OnRefreshMonthlyPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>> OnRefreshUsageByDayOfWeekPlot { get; set; }
    public Action<IEnumerable<double>, IEnumerable<double>> OnRefreshUsageByHourPlot { get; set; }

    #endregion

    #region Methods

    [RelayCommand]
    private async Task OnRefreshDailyHistory()
    {
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.Day));
        Refresh(OnRefreshDailyPlot, points, PlotType.DailyHistory);
    }

    [RelayCommand]
    private async Task OnRefreshMonthlyHistory()
    {
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.Month));
        Refresh(OnRefreshMonthlyPlot, points, PlotType.MonthlyHistory);
    }

    [RelayCommand]
    private async Task OnRefreshUsageByDayOfWeek()
    {
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.DayOfWeek));
        Refresh(OnRefreshUsageByDayOfWeekPlot, points, PlotType.UsageByDayOfWeek, e => (double)e.DayOfWeek);
    }

    [RelayCommand]
    private async Task OnRefreshUsageByHourOfDay()
    {
        var points = await Task.Run(() => _dbRepository.GetUsage(Per.HourOfDay));
        Refresh(OnRefreshUsageByHourPlot, points, PlotType.UsageByHourOfDay, e => e.TimeOfDay.TotalHours);
    }

    private void Refresh(
        Action<IEnumerable<double>, IEnumerable<double>> viewRefresher,
        IEnumerable<DataPoint<DateTime, double>> dataPoints,
        PlotType plotType,
        Func<DateTime, double>? convert = null,
        [CallerMemberName] string? caller = null
    )
    {
        ArgumentNullException.ThrowIfNull(viewRefresher);
        convert ??= r => r.ToOADate();

        _logger.LogDebug("[ViewModel] Refreshing plot {Caller}", caller);
        var points = dataPoints.ToArray();
        var x = points.Select(p => convert(p.X)).ToArray();
        var y = points.Select(p => p.Y).ToArray();
        CurrentPlotType = plotType;
        viewRefresher(x, y);
    }

    #endregion
}