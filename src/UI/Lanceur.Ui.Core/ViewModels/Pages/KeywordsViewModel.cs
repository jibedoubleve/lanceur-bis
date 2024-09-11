using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class KeywordsViewModel : ObservableObject
{
    #region Fields

    private readonly ILogger<KeywordsViewModel> _logger;
    private readonly IStoreService _storeService;
    private readonly IThumbnailManager _thumbnailManager;
    [ObservableProperty] private ObservableCollection<QueryResult> _aliases;
    [ObservableProperty] private QueryResult _selectedAlias;
    [ObservableProperty] private double _percentageLoaded;

    #endregion

    #region Constructors

    public KeywordsViewModel(IStoreService storeService, IThumbnailManager thumbnailManager, ILogger<KeywordsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(storeService);
        ArgumentNullException.ThrowIfNull(logger);

        _storeService = storeService;
        _thumbnailManager = thumbnailManager;
        _logger = logger;
    }

    #endregion

    [RelayCommand]
    private async Task LoadAliases()
    {
        Aliases = [];
        var aliases = _storeService.GetAll()!.ToList();
        _thumbnailManager.RefreshThumbnails(aliases);

        PercentageLoaded = 0;
        var total = (double)aliases.Count;
        var idx = 0d;
        foreach (var alias in aliases)
        {
            Aliases.Add(alias);
            await Task.Delay(1);
            PercentageLoaded = (++idx / total) * 100;
        }
        _logger.LogTrace("Loaded {Count} alias(es)", aliases.Count());
    }
}