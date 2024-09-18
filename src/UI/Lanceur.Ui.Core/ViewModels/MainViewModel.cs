using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    private readonly IExecutionManager _executionManager;
    private readonly IUiUserInteractionService _userInteractionService;

    private Cmdline _lastCriterion = Cmdline.Empty;
    private readonly ILogger<MainViewModel> _logger;
    [ObservableProperty] private string? _query;
    [ObservableProperty] private ObservableCollection<QueryResult> _results = [];
    private readonly ISearchService _searchService;
    [ObservableProperty] private QueryResult? _selectedResult;
    [ObservableProperty] private string? _suggestion;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public MainViewModel(
        ILogger<MainViewModel> logger,
        ISearchService searchService,
        ISettingsFacade settingsFacade,
        IExecutionManager executionManager,
        IUiUserInteractionService userInteractionService
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(settingsFacade);
        ArgumentNullException.ThrowIfNull(executionManager);

        _searchService = searchService;
        _executionManager = executionManager;
        _userInteractionService = userInteractionService;
        _doesReturnAllIfEmpty = settingsFacade.Application.Window.ShowResult;
        _logger = logger;
    }

    #endregion

    #region Methods

    private bool CanSearch() => _lastCriterion.Name != (Cmdline.BuildFromText(Query)?.Name ?? "");

    [RelayCommand]
    private async Task Execute(bool runAsAdmin)
    {
        if (SelectedResult is null) return;

        if(SelectedResult.IsExecutionConfirmationRequired)
        {
            var result = await _userInteractionService.AskAsync("Execute", $"Do you want to execute alias '{SelectedResult.Name}'?");
            if (!result) return;
        } 
        
        _logger.LogTrace("Executing alias {AliasName}", SelectedResult?.Name ?? "<EMPTY>");
        var response = await _executionManager.ExecuteAsync(
            new() { Query = Query, QueryResult = SelectedResult, ExecuteWithPrivilege = runAsAdmin }
        );

        WeakReferenceMessenger.Default.Send(new KeepAliveMessage(response.HasResult));
        if (response.HasResult) Results = new(response.Results);
    }

    private static string GetSuggestion(string query, QueryResult? selectedItem)
    {
        var name = selectedItem?.Name ?? string.Empty;
        return !name.Contains(query) || name.Contains(' ')
            ? string.Empty
            : name;
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
        var index = direction switch
        {
            Direction.Up       => Results.GetPreviousIndex(currentIndex),
            Direction.Down     => Results.GetNextIndex(currentIndex),
            Direction.PageUp   => Results.GetPreviousPage(currentIndex, 9),
            Direction.PageDown => Results.GetNextPage(currentIndex, 9),
            _                  => currentIndex
        };
        SelectedResult = Results.ElementAt(index);
        Suggestion = GetSuggestion(Query ?? "", SelectedResult);
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

    [RelayCommand]
    private void SetQuery()
    {
        if (SelectedResult is null) return;

        var query = Cmdline.BuildFromText(Query);
        var cmd = new Cmdline(SelectedResult.Name, query.Parameters);
        WeakReferenceMessenger.Default.Send<SetQueryMessage>(new(cmd));
    }

    #endregion
}