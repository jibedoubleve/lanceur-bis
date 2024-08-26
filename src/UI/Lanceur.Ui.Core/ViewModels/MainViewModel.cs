using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields

    private readonly bool _doesReturnAllIfEmpty;

    private Cmdline _lastCriterion = Cmdline.Empty;
    private readonly ILogger<MainViewModel> _logger;
    [ObservableProperty] private string? _query;
    [ObservableProperty] private ObservableCollection<QueryResult> _results = [];
    private readonly IAsyncSearchService _searchService;
    [ObservableProperty] private QueryResult? _selectedResult;
    [ObservableProperty] private string? _suggestion;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public MainViewModel(ILogger<MainViewModel> logger, IAsyncSearchService searchService, ISettingsFacade settingsFacade)
    {
        _searchService = searchService;
        _doesReturnAllIfEmpty = settingsFacade.Application.Window.ShowResult;
        _logger = logger;
    }

    #endregion

    #region Methods

    private bool CanSearch() => _lastCriterion.Name != (Cmdline.BuildFromText(Query)?.Name ?? "");

    [RelayCommand]
    private async Task Execute()
    {
        _logger.LogTrace("Executing alias {AliasName}", SelectedResult?.Name ?? "<EMPTY>");
        await Task.CompletedTask;
    }

    private string GetSuggestion(string query, QueryResult? selectedItem)
    {
        var name = selectedItem?.Name ?? string.Empty;
        var suggestion = name.Contains(query) || !name.Contains(' ')
            ? name
            : string.Empty;
        _logger.LogTrace("Suggestion {Suggestion}", suggestion);
        return suggestion;
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task Search()
    {
        var criterion = Cmdline.BuildFromText(Query);

        if (criterion.IsNullOrEmpty()) Results.Clear();

        var results = await _searchService.SearchAsync(criterion, _doesReturnAllIfEmpty);
        Results = new(results);

        SelectedResult = Results.FirstOrDefault()!;
        Suggestion = GetSuggestion(criterion.Name, SelectedResult);

        _lastCriterion = criterion;
        _logger.LogTrace("Found {Count} element(s) for query {Query}", Results.Count, Query);
    }

    #endregion
}