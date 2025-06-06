using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields

    private readonly IExecutionService _executionService;
    private readonly IInteractionHubService _interactionHubService;
    private readonly ILogger<MainViewModel> _logger;
    [ObservableProperty] private string? _query;
    [ObservableProperty] private ObservableCollection<QueryResult> _results = [];
    private readonly ISearchService _searchService;
    [ObservableProperty] private QueryResult? _selectedResult;
    private readonly ISettingsFacade _settingsFacade;
    [ObservableProperty] private string? _suggestion;
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
        IInteractionHubService interactionHubService,
        IWatchdogBuilder watchdogBuilder
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(settingsFacade);
        ArgumentNullException.ThrowIfNull(executionService);
        ArgumentNullException.ThrowIfNull(watchdogBuilder);
        ArgumentNullException.ThrowIfNull(interactionHubService);

        _logger = logger;
        _searchService = searchService;
        _executionService = executionService;
        _interactionHubService = interactionHubService;

        //Settings
        _settingsFacade = settingsFacade;
        WindowBackdropStyle = settingsFacade.Application.Window.BackdropStyle;

        // Configuration
        _watchdog = watchdogBuilder.WithAction(SearchAsync)
                                   .WithInterval(settingsFacade.Application.SearchBox.SearchDelay.Milliseconds())
                                   .Build();
    }

    #endregion

    #region Properties

    private bool DoesReturnAllIfEmpty => _settingsFacade.Application.SearchBox.ShowResult;

    public bool ShowAtStartup => _settingsFacade.Application.SearchBox.ShowAtStartup;

    public bool ShowLastQuery => _settingsFacade.Application.SearchBox.ShowLastQuery;

    #endregion

    #region Methods

    private static string GetSuggestion(string query, QueryResult? selectedItem)
    {
        var name = selectedItem?.Name ?? string.Empty;
        return !name.Contains(query) || name.Contains(' ')
            ? string.Empty
            : name;
    }

    /// <summary>
    ///     Handles the "Tab" key press event, typically used to expand an unfinished query.
    /// </summary>
    [RelayCommand]
    private void OnCompleteQuery()
    {
        if (SelectedResult is null) return;

        var query = Cmdline.Parse(Query);
        var cmd = new Cmdline(SelectedResult.Name, query.Parameters);
        WeakReferenceMessenger.Default.Send<SetQueryMessage>(new(cmd));
    }

    [RelayCommand]
    private async Task OnExecute(bool runAsAdmin)
    {
        try
        {
            if (SelectedResult is null) return;
            if (SelectedResult.IsExecutionConfirmationRequired)
            {
                var result = await _interactionHubService.Interactions.AskAsync($"Do you want to execute alias '{SelectedResult.Name}'?", "Execute");
                if (!result) return;
            }
            
            var response = await _executionService.ExecuteAsync(
                new() { OriginatingQuery = Query, QueryResult = SelectedResult, ExecuteWithPrivilege = runAsAdmin }
            );

            WeakReferenceMessenger.Default.Send(new KeepAliveMessage(response.HasResult));
            if (response.HasResult) Results = new(response.Results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing alias execution");
            _interactionHubService.GlobalNotifications.Error(
                $"An error occured while performing alias execution.{Environment.NewLine}Alias name '{SelectedResult?.Name ?? "<NULL>"}'",
                ex
            );
        }
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

    [RelayCommand]
    private void OnOpenDirectory()
    {
        if (SelectedResult is null) return;

        _logger.LogInformation("Open directory of {Alias}", SelectedResult.Name);
        _executionService.OpenDirectoryAsync(SelectedResult);
    }

    [RelayCommand] private async Task OnSearch() => await _watchdog.Pulse();

    private async Task SearchAsync()
    {
        try
        {
            var criterion = Cmdline.Parse(Query);

            if (criterion.IsNullOrEmpty()) Results.Clear();
            var results = await _searchService.SearchAsync(criterion, DoesReturnAllIfEmpty);
            Results = new(results);
            SelectedResult = Results.FirstOrDefault()!;
            Suggestion = GetSuggestion(criterion.Name, SelectedResult);

            _logger.LogTrace("Found {Count} element(s) for query {Query}", Results.Count, Query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing search");
            _interactionHubService.GlobalNotifications.Error("An error occured while performing search.", ex);
        }
    }

    /// <summary>
    ///     Resets the search UI by clearing both the query input and the displayed results.
    ///     This ensures a clean slate for a new search operation.
    /// </summary>
    public void Clear()
    {
        Query = string.Empty;
        Results.Clear();
    }

    /// <summary>
    ///     Displays search results based on application settings. If allowed, the method will query
    ///     the database for results when the search box is shown. It checks if all aliases should be
    ///     displayed immediately or if results should wait until the user starts typing a query.
    /// </summary>
    public async Task DisplayResultsIfAllowed()
    {
        try
        {
            if (_settingsFacade.Application.SearchBox.ShowResult && Query.IsNullOrWhiteSpace())
            {
                var results = await Task.Run(() => _searchService.SearchAsync(Cmdline.Empty, true));
                Results = new(results);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing search.");
            _interactionHubService.GlobalNotifications.Error("An error occured while performing search.", ex);
        }
    }

    public void RefreshSettings()
    {
        _settingsFacade.Reload();
        WindowBackdropStyle = _settingsFacade.Application.Window.BackdropStyle;
        _watchdog.ResetDelay(_settingsFacade.Application.SearchBox.SearchDelay);
    }

    #endregion
}