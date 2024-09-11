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
    private readonly IStorehService _storehService;
    [ObservableProperty] private ObservableCollection<QueryResult> _aliases;
    [ObservableProperty] private QueryResult _selectedAlias;

    #endregion

    #region Constructors

    public KeywordsViewModel(IStorehService storehService, ILogger<KeywordsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(storehService);
        ArgumentNullException.ThrowIfNull(logger);

        _storehService = storehService;
        _logger = logger;
    }

    #endregion

    [RelayCommand]
    private async Task LoadAliases()
    {
        var aliases = await Task.Run(() => _storehService.GetAll());
        Aliases = new(aliases);
    }
}