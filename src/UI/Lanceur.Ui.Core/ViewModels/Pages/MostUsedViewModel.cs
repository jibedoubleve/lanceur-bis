using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class MostUsedViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<QueryResult> _aliases = [];
    private readonly ILogger<MostUsedViewModel> _logger;
    private readonly IAliasRepository _repository;
    [ObservableProperty] private ObservableCollection<string> _years = [];
    private IEnumerable<UsageQueryResult> _aliasesCache = [];

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

        _aliasesCache = aliases.Result;
        Years = new(yearStr);
    }

    [RelayCommand]
    private async Task OnRefreshAliases(string selectedYear)
    {
        using var _ = _logger.WarnIfSlow(this);
        _logger.LogTrace("Refreshing data for {Year}", selectedYear);

        var a = await Task.Run(
            () =>
            {
                return (int.TryParse(selectedYear, out var year))
                    ? _aliasesCache.Where(x => x.Year >= year)
                    : _aliasesCache;
            }
        );
        Aliases = new(a);
    }

    #endregion
}