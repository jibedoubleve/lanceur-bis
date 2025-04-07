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
    [ObservableProperty] private ObservableCollection<AliasUsageFilter> _filters = [AliasUsageFilter.ShowUsage(), AliasUsageFilter.ShowUnused()];
    private readonly ILogger<MostUsedViewModel> _logger;
    private readonly IAliasRepository _repository;
    [ObservableProperty] private AliasUsageFilter _selectedFilter = AliasUsageFilter.ShowUsage();
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

    private string CacheTag => $"{typeof(MostUsedViewModel).FullName}_{SelectedYear ?? "NoYear"}_{SelectedFilter?.Tag ?? "NoFilter"}";

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
        _logger.LogTrace("Refreshing '{Filter}' for {Year}", SelectedFilter.Tag, SelectedYear);

        var aliases = await _cache.GetOrCreateAsync(
            CacheTag,
            async Task<IEnumerable<UsageQueryResult>> (_) =>
            {
                if (int.TryParse(SelectedYear, out var year))
                    return await Task.Run(
                        () => SelectedFilter.IsUsage()
                            ? _repository.GetMostUsedAliasesByYear(year)
                            : _repository.GetUnusedAliases(year)
                    );

                return await Task.Run(
                    () => SelectedFilter.IsUsage()
                        ? _repository.GetMostUsedAliases()
                        : _repository.GetUnusedAliases()
                );
            },
            CacheEntryOptions.Default
        );

        Aliases = new(aliases ?? []);
    }

    #endregion
}

public record AliasUsageFilter
{
    #region Fields

    private const string UnusedTag = "Unused";
    private const string UsageTag = "Usage";

    #endregion

    #region Constructors

    private AliasUsageFilter(string description, string tag)
    {
        Description = description;
        Tag = tag;
    }

    #endregion

    #region Properties

    public string Description { get; init; }
    public string Tag { get; init; }

    #endregion

    #region Methods

    public bool IsUnused() => Tag == UnusedTag;

    public bool IsUsage() => Tag == UsageTag;
    public static AliasUsageFilter ShowUnused() => new("Show unused aliases", UnusedTag);
    public static AliasUsageFilter ShowUsage() => new("Sort by number of use", UsageTag);

    #endregion
}