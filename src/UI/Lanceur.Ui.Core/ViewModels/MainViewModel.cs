using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields

    private readonly bool _doesReturnAllIfEmpty;
    private readonly IExecutionService _executionService;
    private readonly ILogger<MainViewModel> _logger;
    [ObservableProperty] private string? _query;
    [ObservableProperty] private ObservableCollection<QueryResult> _results = [];
    private readonly ISearchService _searchService;
    [ObservableProperty] private QueryResult? _selectedResult;
    private readonly ISettingsFacade _settingsFacade;
    [ObservableProperty] private string? _suggestion;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IUserInteractionService _userUserInteractionService;
    private readonly IWatchdog _watchdog;
    [ObservableProperty] private string _windowBackdropStyle;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public MainViewModel(
        ILogger<MainViewModel> logger,
        ISearchService searchService,
        ISettingsFacade settingsFacade,
        IExecutionService executionService,
        IUserInteractionService userUserInteractionService,
        IUserNotificationService userNotificationService,
        IWatchdogBuilder watchdogBuilder
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(settingsFacade);
        ArgumentNullException.ThrowIfNull(executionService);
        ArgumentNullException.ThrowIfNull(watchdogBuilder);

        _logger = logger;
        _searchService = searchService;
        _executionService = executionService;
        _userUserInteractionService = userUserInteractionService;
        _userNotificationService = userNotificationService;

        //Settings
        _settingsFacade = settingsFacade;
        _doesReturnAllIfEmpty = settingsFacade.Application.ShowResult;
        _windowBackdropStyle = settingsFacade.Application.Window.BackdropStyle;
        ShowAtStartup = settingsFacade.Application.ShowAtStartup;
        ShowLastQuery = settingsFacade.Application.ShowLastQuery;

        // Configuration
        _watchdog = watchdogBuilder.WithAction(SearchAsync)
                                   .WithInterval(settingsFacade.Application.SearchDelay.Milliseconds())
                                   .Build();
    }

    #endregion

    #region Properties

    public bool ShowAtStartup { get;  }
    public bool ShowLastQuery { get; }

    #endregion

    #region Methods

    private static string GetSuggestion(string query, QueryResult? selectedItem)
    {
        var name = selectedItem?.Name ?? string.Empty;
        return !name.Contains(query) || name.Contains(' ')
            ? string.Empty
            : name;
    }

    [RelayCommand]
    private async Task OnExecute(bool runAsAdmin)
    {
        using var _  = _userNotificationService.TrackLoadingState();
        if (SelectedResult is null) return;

        if (SelectedResult.IsExecutionConfirmationRequired)
        {
            var result = await _userUserInteractionService.AskAsync($"Do you want to execute alias '{SelectedResult.Name}'?", "Execute");
            if (!result) return;
        }

        _logger.LogTrace("Executing alias {AliasName}", SelectedResult?.Name ?? "<EMPTY>");
        var response = await _executionService.ExecuteAsync(
            new() { Query = Query, QueryResult = SelectedResult, ExecuteWithPrivilege = runAsAdmin }
        );

        WeakReferenceMessenger.Default.Send(new KeepAliveMessage(response.HasResult));
        if (response.HasResult) Results = new(response.Results);
    }

    [RelayCommand]
    private void OnNavigate(Direction direction)
    {
        if (SelectedResult == null)
        {
            SelectedResult = Results.FirstOrDefault();
            return;
        }

        const int navOffset = 8;
        var currentIndex = Results.IndexOf(SelectedResult);
        var index = direction switch
        {
            Direction.Up       => Results.GetPreviousIndex(currentIndex),
            Direction.Down     => Results.GetNextIndex(currentIndex),
            Direction.PageUp   => Results.GetPreviousPage(currentIndex, navOffset),
            Direction.PageDown => Results.GetNextPage(currentIndex, navOffset),
            _                  => currentIndex
        };
        SelectedResult = Results.ElementAt(index);
        Suggestion = GetSuggestion(Query ?? "", SelectedResult);
    }

    [RelayCommand] private async Task OnSearch() => await _watchdog.Pulse();

    private async Task SearchAsync()
    {
        var criterion = Cmdline.BuildFromText(Query);

        if (criterion.IsNullOrEmpty()) Results.Clear();
        var results = await _searchService.SearchAsync(criterion, _doesReturnAllIfEmpty);
        Results = new(results);

        SelectedResult = Results.FirstOrDefault()!;
        Suggestion = GetSuggestion(criterion.Name, SelectedResult);

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

    public void DisplayResultsIfAllowed()
    {
        if (_settingsFacade.Application.ShowResult)
            _ = Task.Run(() => _searchService.SearchAsync(Cmdline.Empty, true))
                    .ContinueWith(r => Results = new(r.Result), TaskScheduler.FromCurrentSynchronizationContext());

        _logger.LogTrace("When showing search, display all results: {ShowAtStartup}", _settingsFacade.Application.ShowAtStartup);
    }

    public void RefreshSettings()
    {
        _settingsFacade.Reload();
        WindowBackdropStyle = _settingsFacade.Application.Window.BackdropStyle;
        _watchdog.ResetDelay(_settingsFacade.Application.SearchDelay);
    }

    #endregion
}