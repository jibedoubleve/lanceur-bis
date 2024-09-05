using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Everything.Wrapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Messages;
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
    private readonly IExecutionManager _executionManager;
    [ObservableProperty] private QueryResult? _selectedResult;
    [ObservableProperty] private string? _suggestion;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public MainViewModel(ILogger<MainViewModel> logger, IAsyncSearchService searchService, ISettingsFacade settingsFacade, IExecutionManager executionManager)
    {
        ArgumentNullException.ThrowIfNull(nameof(logger));
        ArgumentNullException.ThrowIfNull(nameof(searchService));
        ArgumentNullException.ThrowIfNull(nameof(settingsFacade));
        ArgumentNullException.ThrowIfNull(nameof(executionManager));

        _searchService = searchService;
        _executionManager = executionManager;
        _doesReturnAllIfEmpty = settingsFacade.Application.Window.ShowResult;
        _logger = logger;
    }

    #endregion

    #region Methods

    private bool CanSearch() => _lastCriterion.Name != (Cmdline.BuildFromText(Query)?.Name ?? "");

    [RelayCommand]
    private void Suggest()
    {
        var cmd = Cmdline.BuildFromText(Query);
        Query = cmd with { Name = Suggestion };
    }

    [RelayCommand]
    private async Task Execute(bool runAsAdmin)
    {
        _logger.LogTrace("Executing alias {AliasName}", SelectedResult?.Name ?? "<EMPTY>");
        var response = await _executionManager.ExecuteAsync(
            new()
            {
                Query = Query,
                QueryResult = SelectedResult,
                ExecuteWithPrivilege = runAsAdmin
            }
        );

        WeakReferenceMessenger.Default.Send(new KeepAliveRequest(response.HasResult));
        if (response.HasResult) Results = new(response.Results);
    }

    [RelayCommand]
    private void Navigate(Direction direction)
    {
        if (SelectedResult == null)
        {
            SelectedResult = Results.FirstOrDefault();
            return;
        }

        var currentIndex = Results.IndexOf(SelectedResult);
        var index  = direction switch
        {
            Direction.Up      => Results.GetPreviousIndex(currentIndex),
            Direction.Down    => Results.GetNextIndex(currentIndex),
            Direction.PageUp => 1,
            Direction.PageDown => 1,
            _                 => currentIndex,
        };
        SelectedResult = Results.ElementAt(index);
        Suggestion = SelectedResult?.Name;
    }

    private static string GetSuggestion(string query, QueryResult? selectedItem)
    {
        var name = selectedItem?.Name ?? string.Empty;
        return name.Contains(query) || !name.Contains(' ')
            ? name
            : string.Empty;
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