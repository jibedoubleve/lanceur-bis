using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class MostUsedViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<QueryResult> _aliases = new();
    private readonly IAliasRepository _repository;
    private readonly ILogger<MostUsedViewModel> _logger;

    #endregion

    #region Constructors

    public MostUsedViewModel(ILogger<MostUsedViewModel> logger, IAliasRepository repository)
    {
        _repository = repository;
        _logger = logger;
    }

    #endregion

    #region Methods

    [RelayCommand]
    private async Task OnLoadAliases()
    {
        _logger.LogDebug("Load data for {ViewModelType}", typeof(MostUsedViewModel));
        var aliases = await Task.Run(() => _repository.GetMostUsedAliases());
        Aliases = new(aliases);
    }

    #endregion
}