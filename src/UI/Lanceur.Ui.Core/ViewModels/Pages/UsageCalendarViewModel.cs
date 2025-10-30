using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class UsageCalendarViewModel : ObservableObject
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    [ObservableProperty] private ObservableCollection<AliasUsageItem> _history = [];

    private readonly ILogger<UsageCalendarViewModel> _logger;
    [ObservableProperty] private string _selectedDay;
    private readonly IThumbnailService _thumbnailService;

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
    private async Task OnLoadHistory(DateTime selectedDay)
    {
        var history = await Task.Run(() => _aliasRepository.GetUsageFor(selectedDay));

        SelectedDay = selectedDay.ToString("dddd dd MMMM yyyy");
        History = new(history);
    }

    [RelayCommand]
    private void OnLoadThumbnail(AliasUsageItem? usageItem)
    {
        if (usageItem is null) return;

        var queryResult = usageItem.ToAliasQueryResult();

        if (!queryResult.Thumbnail.IsNullOrEmpty()) return; /* Already loaded */

        try
        {
            _thumbnailService.UpdateThumbnail(queryResult);
            foreach (var item in History)
            {
                if(item.Id == usageItem.Id) { item.Thumbnail = queryResult.Thumbnail; }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load thumbnail for alias id {IdAlias}", queryResult.Id);
        }
    }

    #endregion
}