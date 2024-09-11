using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class KeywordsViewModel : ObservableObject
{
    #region Fields

    private readonly ILogger<KeywordsViewModel> _logger;
    private readonly ISearchService _searchService;
    [ObservableProperty] private ObservableCollection<QueryResult> _aliases;
    [ObservableProperty] private QueryResult _selectedAlias;

    #endregion

    #region Constructors

    public KeywordsViewModel(ISearchService searchService, ILogger<KeywordsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(logger);

        _searchService = searchService;
        _logger = logger;
    }

    #endregion

    [RelayCommand]
    private async Task LoadAliases()
    {
        var aliases = await Task.Run(() => _searchService.GetAll());
        Aliases = new(aliases);
    }
}