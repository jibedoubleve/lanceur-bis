using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class UsageCalendarViewModel : ObservableObject
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    [ObservableProperty] private DateTime _displayDateEnd;
    [ObservableProperty] private DateTime _displayDateStart;

    [ObservableProperty]
    private string _displayDateTitle = DateTime.Today.ToString(DatePattern, CultureInfo.InvariantCulture);

    [ObservableProperty] private ObservableCollection<AliasUsageItem> _history = [];

    private readonly ILogger<UsageCalendarViewModel> _logger;
    private readonly IThumbnailService _thumbnailService;

    private const string DatePattern = "dddd dd MMMM yyyy";

    #endregion

    #region Constructors

    public UsageCalendarViewModel(
        ILogger<UsageCalendarViewModel> logger,
        IAliasRepository aliasRepository,
        IThumbnailService thumbnailService
    )
    {
        _logger = logger;
        _aliasRepository = aliasRepository;
        _thumbnailService = thumbnailService;
    }

    #endregion

    #region Methods

    [RelayCommand]
    private async Task OnLoadAsync()
    {
        var date = await Task.Run(() => _aliasRepository.GetFirstHistory());
        DisplayDateStart = date ??  DateTime.Today;
        DisplayDateEnd = DateTime.Today;
        DisplayDateTitle = DateTime.Today.ToString(DatePattern, CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task OnLoadHistory(DateTime selectedDay)
    {
        var history = await Task.Run(() => _aliasRepository.GetUsageFor(selectedDay));

        DisplayDateTitle = selectedDay.ToString(DatePattern, CultureInfo.InvariantCulture);
        History = new(history);
    }

    [RelayCommand]
    private void OnLoadThumbnail(AliasUsageItem? usageItem)
    {
        if (usageItem is null) { return; }

        var queryResult = usageItem.ToAliasQueryResult();

        if (!queryResult.Thumbnail.IsNullOrEmpty()) { return; /* Already loaded */ }

        try
        {
            _thumbnailService.UpdateThumbnail(queryResult);
            foreach (var item in History)
                if (item.Id == usageItem.Id) { item.Thumbnail = queryResult.Thumbnail; }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load thumbnail for alias id {IdAlias}", queryResult.Id);
        }
    }

    public IEnumerable<DateTime> GetHistoryOfMonth(DateTime? selectedDay)
        => selectedDay is not null
            ? _aliasRepository.GetDaysWithHistory(selectedDay.Value)
            : [];

    #endregion
}