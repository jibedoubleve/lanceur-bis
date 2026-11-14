using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class AnalyticsViewModel : ObservableObject
{
    #region Fields

    private readonly IChartDefinitionService _chartService;

    private readonly ILogger<AnalyticsViewModel> _logger;

    #endregion

    #region Constructors

    public AnalyticsViewModel(ILogger<AnalyticsViewModel> logger, IChartDefinitionService chartService)
    {
        _logger = logger;
        _chartService = chartService;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    public partial ObservableCollection<ChartDescriptorViewModel> Charts { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<ChartFilterViewModel> Filters { get; set; } = [];

    public ChartDescriptorViewModel? SelectedChart
    {
        get;
        set
        {
            if (value?.Filters is not null)
            {
                Filters = [.. value.Filters];
            }

            field = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial ChartDataViewModel? SelectedChartData { get; set; }

    #endregion

    #region Methods

    [RelayCommand]
    private async Task OnLoadChartAsync()
    {
        if (SelectedChart is null) { return; }

        var filters = Filters.ToDictionary(k => k.ParamName, v => v.CurrentOption);
        
        _logger.LogDebug("Filters to apply:  {Filters}", filters);

        var points = await Task.Run(() => _chartService.FetchChartPoints(SelectedChart.Id, filters));

        SelectedChartData = new ChartDataViewModel(SelectedChart, points);

        _logger.LogInformation(
            "Loaded {Count} points for chart {Chart}",
            points.Count,
            SelectedChart.Title
        );
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        var charts = await Task.Run(() => _chartService.FetchChartDefinitions());
        _logger.LogInformation("Loaded {Count} chart definitions", charts.Count);

        Charts = [.. charts.ToViewModels()];
        SelectedChart = Charts.FirstOrDefault();
    }

    #endregion
}