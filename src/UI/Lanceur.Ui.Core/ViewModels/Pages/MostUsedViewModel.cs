using System.Collections.ObjectModel;
using System.Web.Bookmarks.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class MostUsedViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<UsageQueryResult> _aliases = [];
    private readonly IMemoryCache _cache;
    private readonly ILogger<MostUsedViewModel> _logger;
    private readonly IAliasRepository _repository;
    [ObservableProperty] private string? _selectedYear;
    [ObservableProperty] private ObservableCollection<string> _years = [];

    #endregion

    #region Constructors

    public MostUsedViewModel(ILogger<MostUsedViewModel> logger, IAliasRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    #endregion

    #region Properties

    private string CacheTag => $"{typeof(MostUsedViewModel).FullName}_MostUsedAliasOfYear_{SelectedYear ?? "NoYear"}";

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

        Years = new(yearStr);
    }

    [RelayCommand]
    private async Task OnRefreshAliases()
    {
        using var _ = _logger.WarnIfSlow(this);
        _logger.LogTrace("Refreshing most used alias for {Year}", SelectedYear);

        var aliases = await _cache.GetOrCreateAsync(
            CacheTag,
            async Task<IEnumerable<UsageQueryResult>> (_) =>
            {
                if (int.TryParse(SelectedYear, out var year))
                {
                    return await Task.Run(() =>   _repository.GetMostUsedAliasesByYear(year)
                    );
                }

                return await Task.Run(() => _repository.GetMostUsedAliases());
            },
            CacheEntryOptions.Default
        );

        Aliases = new(aliases ?? []);
    }

    #endregion
}