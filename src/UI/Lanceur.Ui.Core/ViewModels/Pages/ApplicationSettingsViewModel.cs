using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores.Everything;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class ApplicationSettingsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _apiToken = string.Empty;
    [ObservableProperty] private string _bookmarkSourceBrowser = string.Empty;
    [ObservableProperty] private int _cpuSmoothingIndex;
    [ObservableProperty] private string _dbPath = string.Empty;
    private readonly IEnigma _enigma;
    private readonly IStoreShortcutService _storeShortcutService;
    private readonly ISettingsProviderFacade _configuration;
    [ObservableProperty] private bool _excludeFilesInBinWithEverything;
    [ObservableProperty] private bool _excludeHiddenFilesWithEverything;
    [ObservableProperty] private bool _excludeSystemFilesWithEverything;
    [ObservableProperty] private ObservableCollection<FeatureFlag> _featureFlags = [];
    private readonly IUserCommunicationService _hubService;
    [ObservableProperty] private bool _includeOnlyExecFilesWithEverything;
    [ObservableProperty] private bool _isAdminModeEnabled;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isResourceMonitorEnabled;
    [ObservableProperty] private bool _isSettingsButtonEnabled;
    [ObservableProperty]private bool _isStatusBarAlwaysVisible;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    [ObservableProperty] private int _notificationDisplayDuration;
    [ObservableProperty] private int _refreshRate;
    [ObservableProperty] private double _searchDelay;
    [ObservableProperty] private LogLevel _selectedLogLevel;
    [ObservableProperty] private bool _showAtStartup;
    [ObservableProperty] private bool _showLastQuery;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private ObservableCollection<StoreShortcut> _storeShortcuts = [];
    [ObservableProperty] private bool _toggleVisibility;
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle = "Mica";
    [ObservableProperty] private string _dateTimeFormat = ""; // Just to avoid warning, this prop is set in the Ctor
    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserCommunicationService userCommunicationService,
        ILogger<ApplicationSettingsViewModel> logger,
        IAppRestartService appRestartService,
        IViewFactory viewFactory,
        IEnigma enigma,
        IStoreShortcutService storeShortcutService,
        ISettingsProviderFacade configuration)
    {
        ArgumentNullException.ThrowIfNull(userCommunicationService);
        ArgumentNullException.ThrowIfNull(appRestartService);
        ArgumentNullException.ThrowIfNull(viewFactory);


        _hubService = userCommunicationService;
        _logger = logger;
        _viewFactory = viewFactory;
        _enigma = enigma;
        _storeShortcutService = storeShortcutService;
        _configuration = configuration;

        // Hotkey
        var hk = _configuration.Application.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        SelectedLogLevel = _configuration.Infrastructure.MinimumLogLevel;

        // Miscellaneous
        MapSettingsFromDbToUi();
        var featureFlags = _configuration.Application.FeatureFlags.ToArray();
        IsResourceMonitorEnabled = featureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Setup behaviour on property changed
        foreach (var flag in FeatureFlags)
        {
            flag.PropertyChanged += OnPropertyChanged;
        }

        PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Properties

    public static ObservableCollection<LogLevel> LogLevels => [LogLevel.Trace, LogLevel.Debug, LogLevel.Information];

    #endregion

    #region Methods

    private int GetHotKey()
    {
        var result = 0;
        if (IsCtrl) { result += (int)ModifierKeys.Control; }

        if (IsAlt) { result += (int)ModifierKeys.Alt; }

        if (IsWin) { result += (int)ModifierKeys.Windows; }

        if (IsShift) { result += (int)ModifierKeys.Shift; }

        return result;
    }

    private void MapSettingsFromDbToUi()
    {
        DbPath = _configuration.Infrastructure.DbPath;

        // Search box section
        SearchDelay = _configuration.Application.SearchBox.SearchDelay;
        ShowResult = _configuration.Application.SearchBox.ShowResult;
        ShowAtStartup = _configuration.Application.SearchBox.ShowAtStartup;
        ShowLastQuery = _configuration.Application.SearchBox.ShowLastQuery;
        ToggleVisibility = _configuration.Application.SearchBox.ToggleVisibility;
        
        IsSettingsButtonEnabled = _configuration.Application.SearchBox.IsSettingsButtonEnabled;
        IsStatusBarAlwaysVisible = _configuration.Application.SearchBox.IsStatusBarAlwaysVisible;

        // Store section
        BookmarkSourceBrowser = _configuration.Application.Stores.BookmarkSourceBrowser;

        var shortcuts = _storeShortcutService.Resolve(_configuration.Application.Stores);
        StoreShortcuts = new ObservableCollection<StoreShortcut>(shortcuts);

        // Everything Store
        var adapter = new EverythingQueryAdapter(_configuration.Application.Stores.EverythingQuerySuffix);
        ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        NotificationDisplayDuration = _configuration.Application.Window.NotificationDisplayDuration;
        WindowBackdropStyle = _configuration.Application.Window.BackdropStyle;
        DateTimeFormat = _configuration.Application.Window.DateTimeFormat;

        // Feature flags
        FeatureFlags = new ObservableCollection<FeatureFlag>(_configuration.Application.FeatureFlags);

        // Resource Monitor
        CpuSmoothingIndex = _configuration.Application.ResourceMonitor.CpuSmoothingIndex;
        RefreshRate = _configuration.Application.ResourceMonitor.RefreshRate;

        // Miscellaneous
        var token = _configuration.Application.Github.Token;
        ApiToken = token.IsNullOrWhiteSpace() ? string.Empty : _enigma.Decrypt(token!);
    }

    private void MapSettingsFromUiToDb()
    {
        _configuration.Infrastructure.DbPath = DbPath;

        // Search box section
        _configuration.Application.SearchBox.SearchDelay = SearchDelay;
        _configuration.Application.SearchBox.ShowResult = ShowResult;
        _configuration.Application.SearchBox.ShowAtStartup = ShowAtStartup;
        _configuration.Application.SearchBox.ShowLastQuery = ShowLastQuery;
        _configuration.Application.SearchBox.ToggleVisibility = ToggleVisibility;
        
        _configuration.Application.SearchBox.IsSettingsButtonEnabled = IsSettingsButtonEnabled;
        _configuration.Application.SearchBox.IsStatusBarAlwaysVisible = IsStatusBarAlwaysVisible;

        // Store section
        _configuration.Application.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        _configuration.Application.Stores.StoreShortcuts = StoreShortcuts.ToArray();

        // Everything Store
        var query = new EverythingQueryBuilder();
        if (ExcludeHiddenFilesWithEverything) { query.ExcludeHiddenFiles(); }
        if (ExcludeSystemFilesWithEverything) { query.ExcludeSystemFiles(); }
        if (IncludeOnlyExecFilesWithEverything) { query.OnlyExecFiles(); }
        if (ExcludeFilesInBinWithEverything) { query.ExcludeFilesInBin(); }

        _configuration.Application.Stores.EverythingQuerySuffix = query.BuildQuery();

        // Window section
        _configuration.Application.Window.NotificationDisplayDuration = NotificationDisplayDuration;
        _configuration.Application.Window.BackdropStyle = WindowBackdropStyle;
        _configuration.Application.Window.DateTimeFormat = DateTimeFormat;

        // Feature flags
        _configuration.Application.FeatureFlags = FeatureFlags;
        IsResourceMonitorEnabled = FeatureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Resource Monitor
        _configuration.Application.ResourceMonitor.CpuSmoothingIndex = CpuSmoothingIndex;
        _configuration.Application.ResourceMonitor.RefreshRate = RefreshRate;

        // Miscellaneous
        _configuration.Application.Github.Token
            = ApiToken.IsNullOrWhiteSpace() ? string.Empty : _enigma.Encrypt(ApiToken);
    }

    [RelayCommand]
    private async Task OnEditStoreShortcut(StoreShortcut? storeShortcut)
    {
        if (storeShortcut is null)
        {
            _logger.LogWarning("No store shortcut selected.");
            return;
        }

        var view = _viewFactory.CreateView(storeShortcut);
        var storeName = storeShortcut.StoreType
                                     ?.Replace("Lanceur.Infra.Stores.", "")
                                     .Replace("Store", "");

        var savedAliasOverride = storeShortcut.AliasOverride;
        var result = await _hubService.Dialogues.AskUserYesNoAsync(
            view,
            ButtonLabels.Apply,
            ButtonLabels.Cancel,
            $"Edit shortcut for store '{storeName}'"
        );

        if (!result)
        {
            storeShortcut.AliasOverride = savedAliasOverride;
            return;
        }

        SaveSettings();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        string[] properties = [nameof(IsResourceMonitorEnabled), nameof(IsAdminModeEnabled)];
        if (properties.Contains(e.PropertyName)) { return; }

        _logger.LogTrace("Property {Property} changed", e.PropertyName);
        SaveSettings();
    }

    internal void SaveSettings()
    {
        var hash = _configuration.Application.HotKey.GetHashCode();
        _configuration.Application.HotKey = new HotKeySection(
            GetHotKey(),
            Key
        );

        List<bool> reboot =
        [
            hash != _configuration.Application.HotKey.GetHashCode(),
            _configuration.Infrastructure.DbPath != DbPath,
            _configuration.Infrastructure.MinimumLogLevel != SelectedLogLevel
        ];

        MapSettingsFromUiToDb();
        _configuration.Infrastructure.MinimumLogLevel = SelectedLogLevel;
        _configuration.Save();

        var needRestart = reboot.Any(r => r);

        _logger.LogTrace("Saved settings. Need restart {NeedRestart}", needRestart);

        if (needRestart) { _hubService.GlobalNotifications.AskRestart(); }
    }

    #endregion
}