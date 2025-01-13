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
    private readonly ILogger<MostUsedViewModel> _logger;
    private readonly IAliasRepository _repository;
    [ObservableProperty] private ObservableCollection<string> _years = new();

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

        var aliases = Task.Run(() => _repository.GetMostUsedAliases());
        var years = Task.Run(() => _repository.GetYearsWithUsage());

        await Task.WhenAll(aliases, years);

        var yearStr = years.Result
                           .Select(e => e.ToString())
                           .ToList();
        yearStr.Insert(0, "All time usage");
        
        Aliases = new(aliases.Result);
        Years = new(yearStr);
    }

    [RelayCommand]
    private async Task OnRefreshAliases(string selectedYear)
    {
        var parsed = int.TryParse(selectedYear, out var valYear);
        _logger.LogTrace("Parsing succeeded: {Parsed}. Value: {ValYear}", parsed, valYear);
            
            
        _logger.LogTrace("Refreshing data for {Year}", selectedYear);
        var aliases = int.TryParse(selectedYear, out var year)
            ? await Task.Run(() => _repository.GetMostUsedAliases(year))
            : await Task.Run(() => _repository.GetMostUsedAliases());
        Aliases = new(aliases);
    }

    #endregion
}