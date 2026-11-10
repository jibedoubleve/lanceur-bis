using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields

    private readonly IExecutionService _executionService;
    private readonly QueryInputTracker _inputTracker = new();

    private bool _isSearchActivated = true;

    private readonly ILogger<MainViewModel> _logger;
    private readonly ISearchService _searchService;
    private readonly ISection<SearchBoxSection> _stgSearchBox;
    private readonly ISection<WindowSection> _stgWindow;
    private readonly IThumbnailService _thumbnailService;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly IWatchdog _watchdog;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public MainViewModel(
        ILogger<MainViewModel> logger,
        ISearchService searchService,
        ISection<WindowSection> stgWindow,
        ISection<SearchBoxSection> stgSearchBox,
        IExecutionService executionService,
        IUserCommunicationService userCommunicationService,
        IWatchdogBuilder watchdogBuilder,
        IThumbnailService thumbnailService
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(stgWindow);
        ArgumentNullException.ThrowIfNull(stgSearchBox);
        ArgumentNullException.ThrowIfNull(executionService);
        ArgumentNullException.ThrowIfNull(watchdogBuilder);
        ArgumentNullException.ThrowIfNull(userCommunicationService);
        ArgumentNullException.ThrowIfNull(thumbnailService);

        _logger = logger;
        _searchService = searchService;
        _executionService = executionService;
        _userCommunicationService = userCommunicationService;
        _thumbnailService = thumbnailService;

        //Settings
        _stgWindow = stgWindow;
        _stgSearchBox = stgSearchBox;
        WindowBackdropStyle = stgWindow.Value.BackdropStyle;

        // Configuration
        _watchdog = watchdogBuilder.WithAction(SearchAsync)
                                   .WithInterval(_stgSearchBox.Value.SearchDelay.Milliseconds())
                                   .Build();
    }

    #endregion

    #region Properties

    private bool DoesReturnAllIfEmpty => _stgSearchBox.Value.ShowResult;

    [ObservableProperty]
    public partial string? Query { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QueryResult> Results { get; set; } = [];

    [ObservableProperty]
    public partial QueryResult? SelectedResult { get; set; }

    public bool ShowAtStartup => _stgSearchBox.Value.ShowAtStartup;

    public bool ShowLastQuery => _stgSearchBox.Value.ShowLastQuery;

    public bool ShowSettingsButton => _stgSearchBox.Value.IsSettingsButtonEnabled;

    public bool ShowStatusBar =>
        _stgSearchBox.Value.IsStatusBarAlwaysVisible
        || Results.Count > 0;

    [ObservableProperty]
    public partial string? Suggestion { get; set; }

    [ObservableProperty]
    public partial string WindowBackdropStyle { get; set; }

    #endregion

    #region Methods

    private static bool CanNavigate(Direction direction) => direction != Direction.None;

    private string GetSuggestion(QueryResult? selectedItem)
    {
        if (Query is null
            || (!selectedItem?.IsResult ?? true)
            || !selectedItem!.Name.StartsWith(Cmdline.Parse(Query).Name, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }


        var name = selectedItem.Name;

        var trimmed = Query.TrimStart();
        var trimmedLength = Query.Length - trimmed.Length;
        var leading = Query[..trimmedLength];

        return leading + name;
    }

    /// <summary>
    ///     Handles the "Tab" key press event, typically used to expand an unfinished query.
    /// </summary>
    [RelayCommand]
    private void OnCompleteQuery()
    {
        if (SelectedResult is null) { return; }

        var query = Cmdline.Parse(Query);
        var cmd = new Cmdline(SelectedResult.Name, query.Parameters);
        WeakReferenceMessenger.Default.Send(new SetQueryMessage(cmd));
    }

    [RelayCommand]
    private async Task OnExecute(bool runAsAdmin)
    {
        try
        {
            await _watchdog.Flush();

            if (SelectedResult is null) { return; }

            if (SelectedResult.IsExecutionConfirmationRequired)
            {
                var result = await _userCommunicationService.Dialogues.AskAsync(
                    $"Do you want to execute alias '{SelectedResult.Name}'?",
                    "Execute"
                );
                if (!result) { return; }
            }

            var response = await _executionService.ExecuteAsync(
                new ExecutionRequest(SelectedResult, Cmdline.Parse(Query), runAsAdmin)
            );

            WeakReferenceMessenger.Default.Send(new KeepAliveMessage(response.HasResult));
            if (response.HasResult) { Results = [.. response.Results]; }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing alias execution");
            _userCommunicationService.GlobalNotifications.Error(
                $"An error occured while performing alias execution.{Environment.NewLine}Alias name '{SelectedResult?.Name ?? "<NULL>"}'",
                ex
            );
        }
    }

    [RelayCommand]
    private void OnLoadThumbnail(QueryResult? queryResult)
    {
        if (queryResult is null) { return; }

        try { _thumbnailService.UpdateThumbnail(queryResult); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load thumbnail for alias id {IdAlias}", queryResult.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigate))]
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
        SelectedResult = Results[index];
        Suggestion = GetSuggestion(SelectedResult);
        SendAlignParameter(Suggestion, _inputTracker.Current);
    }

    [RelayCommand]
    private void OnOpenDirectory()
    {
        if (SelectedResult is null) { return; }

        _logger.LogInformation("Open directory of {Alias}", SelectedResult.Name);
        _executionService.OpenDirectory(SelectedResult);
    }

    [RelayCommand]
    private async Task OnSearch()
    {
        if (!_isSearchActivated) { return; }

        await _watchdog.Pulse();
    }

    private async Task SearchAsync()
    {
        try
        {
            var report = TrackCurrentQuery();

            switch (report)
            {
                case TrackingReport.Empty when !DoesReturnAllIfEmpty:
                    Clear();
                    break;

                case TrackingReport.EnteringParameters:
                    SendAlignParameter(Suggestion ?? string.Empty, _inputTracker.Current);
                    break;

                case TrackingReport.Deleting:
                    WeakReferenceMessenger.Default.Send(new SetQueryMessage(_inputTracker.Current.Raw));
                    break;
            }

            await _searchService.SearchAsync(Results, Query!, DoesReturnAllIfEmpty);
            SelectedResult = Results.FirstOrDefault()!;
            Suggestion = GetSuggestion(SelectedResult);

            _logger.LogTrace("Found {Count} element(s) for query {Query}", Results.Count, Query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing search");
            _userCommunicationService.GlobalNotifications.Error("An error occured while performing search.", ex);
        }
    }

    /// <summary>
    ///     Notify the UI it has to align the arguments to avoid the query
    ///     to overlap with the suggestion
    /// </summary>
    private void SendAlignParameter(string suggestion, Cmdline current)
    {
        // Nothing to align with: keep the query as typed, otherwise the whitespace
        // separating the name from the parameters is removed (e.g. "st aim" -> "staim").
        if (suggestion.IsNullOrEmpty()) { return; }

        _isSearchActivated = false;
        var value = new AlignParametersValue(suggestion, current);
        WeakReferenceMessenger.Default.Send(new AlignParametersMessage(value));
        _isSearchActivated = true;
    }

    private TrackingReport TrackCurrentQuery() => _inputTracker.Track(Query);

    /// <summary>
    ///     Resets the search UI by clearing both the query input and the displayed results.
    ///     This ensures a clean slate for a new search operation.
    /// </summary>
    public void Clear()
    {
        Query = string.Empty;
        Suggestion = string.Empty;
        _inputTracker.Reset();
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
            if (_stgSearchBox.Value.ShowResult && Query.IsNullOrWhiteSpace())
            {
                await _searchService.SearchAsync(Results, Cmdline.Empty, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while performing search.");
            _userCommunicationService.GlobalNotifications.Error("An error occured while performing search.", ex);
        }
    }

    public void RefreshSettings()
    {
        _stgSearchBox.Reload();

        WindowBackdropStyle = _stgWindow.Value.BackdropStyle;

        // Now the values are refreshed, let's refresh the ui
        OnPropertyChanged(nameof(ShowStatusBar));
        OnPropertyChanged(nameof(ShowSettingsButton));

        _watchdog.ResetDelay(_stgSearchBox.Value.SearchDelay);
    }

    #endregion
}