using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Configuration.Configurations;
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
    private readonly ISettingsProvider<ApplicationSettings> _appSettings;
    [ObservableProperty] private string _bookmarkSourceBrowser = string.Empty;
    [ObservableProperty] private int _cpuSmoothingIndex;
    [ObservableProperty] private string _dateTimeFormat = ""; // Just to avoid warning, this prop is set in the Ctor
    [ObservableProperty] private string _dbPath = string.Empty;
    private readonly IEnigma _enigma;
    [ObservableProperty] private bool _excludeFilesInBinWithEverything;
    [ObservableProperty] private bool _excludeHiddenFilesWithEverything;
    [ObservableProperty] private bool _excludeSystemFilesWithEverything;
    [ObservableProperty] private ObservableCollection<FeatureFlag> _featureFlags = [];
    private readonly IUserCommunicationService _hubService;
    [ObservableProperty] private bool _includeOnlyExecFilesWithEverything;
    private readonly ISettingsProvider<InfrastructureSettings> _infraSettings;
    [ObservableProperty] private bool _isAdminModeEnabled;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isResourceMonitorEnabled;
    [ObservableProperty] private bool _isSettingsButtonEnabled;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private bool _isStatusBarAlwaysVisible;
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
    private readonly IStoreShortcutService _storeShortcutService;
    [ObservableProperty] private bool _toggleVisibility;
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle = "Mica";

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserCommunicationService userCommunicationService,
        ILogger<ApplicationSettingsViewModel> logger,
        IAppRestartService appRestartService,
        IViewFactory viewFactory,
        IEnigma enigma,
        IStoreShortcutService storeShortcutService,
        ISettingsProvider<ApplicationSettings> appSettings,
        ISettingsProvider<InfrastructureSettings> infraSettings)
    {
        ArgumentNullException.ThrowIfNull(userCommunicationService);
        ArgumentNullException.ThrowIfNull(appRestartService);
        ArgumentNullException.ThrowIfNull(viewFactory);


        _hubService = userCommunicationService;
        _logger = logger;
        _viewFactory = viewFactory;
        _enigma = enigma;
        _storeShortcutService = storeShortcutService;
        _appSettings = appSettings;
        _infraSettings = infraSettings;

        // Hotkey
        var hk = _appSettings.Current.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        SelectedLogLevel = infraSettings.Current.Logging.MinimumLogLevel;

        // Miscellaneous
        MapSettingsFromDbToUi();
        var featureFlags = appSettings.Current.FeatureFlags.ToArray();
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
        DbPath = _infraSettings.Current.Database.DbPath;

        // Search box section
        SearchDelay = _appSettings.Current.SearchBox.SearchDelay;
        ShowResult = _appSettings.Current.SearchBox.ShowResult;
        ShowAtStartup = _appSettings.Current.SearchBox.ShowAtStartup;
        ShowLastQuery = _appSettings.Current.SearchBox.ShowLastQuery;
        ToggleVisibility = _appSettings.Current.SearchBox.ToggleVisibility;

        IsSettingsButtonEnabled = _appSettings.Current.SearchBox.IsSettingsButtonEnabled;
        IsStatusBarAlwaysVisible = _appSettings.Current.SearchBox.IsStatusBarAlwaysVisible;

        // Store section
        BookmarkSourceBrowser = _appSettings.Current.Stores.BookmarkSourceBrowser;

        var shortcuts = _storeShortcutService.Resolve(_appSettings.Current.Stores);
        StoreShortcuts = new ObservableCollection<StoreShortcut>(shortcuts);

        // Everything Store
        var adapter = new EverythingQueryAdapter(_appSettings.Current.Stores.EverythingQuerySuffix);
        ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        NotificationDisplayDuration = _appSettings.Current.Window.NotificationDisplayDuration;
        WindowBackdropStyle = _appSettings.Current.Window.BackdropStyle;
        DateTimeFormat = _appSettings.Current.Window.DateTimeFormat;

        // Feature flags
        FeatureFlags = new ObservableCollection<FeatureFlag>(_appSettings.Current.FeatureFlags);

        // Resource Monitor
        CpuSmoothingIndex = _appSettings.Current.ResourceMonitor.CpuSmoothingIndex;
        RefreshRate = _appSettings.Current.ResourceMonitor.RefreshRate;

        // Miscellaneous
        var token = _appSettings.Current.Github.Token;
        ApiToken = token.IsNullOrWhiteSpace() ? string.Empty : _enigma.Decrypt(token!);
    }

    private void MapSettingsFromUiToDb()
    {
        _infraSettings.Current.Database.DbPath = DbPath;

        // Search box section
        _appSettings.Current.SearchBox.SearchDelay = SearchDelay;
        _appSettings.Current.SearchBox.ShowResult = ShowResult;
        _appSettings.Current.SearchBox.ShowAtStartup = ShowAtStartup;
        _appSettings.Current.SearchBox.ShowLastQuery = ShowLastQuery;
        _appSettings.Current.SearchBox.ToggleVisibility = ToggleVisibility;

        _appSettings.Current.SearchBox.IsSettingsButtonEnabled = IsSettingsButtonEnabled;
        _appSettings.Current.SearchBox.IsStatusBarAlwaysVisible = IsStatusBarAlwaysVisible;

        // Store section
        _appSettings.Current.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        _appSettings.Current.Stores.StoreShortcuts = StoreShortcuts.ToArray();

        // Everything Store
        var query = new EverythingQueryBuilder();
        if (ExcludeHiddenFilesWithEverything) { query.ExcludeHiddenFiles(); }

        if (ExcludeSystemFilesWithEverything) { query.ExcludeSystemFiles(); }

        if (IncludeOnlyExecFilesWithEverything) { query.OnlyExecFiles(); }

        if (ExcludeFilesInBinWithEverything) { query.ExcludeFilesInBin(); }

        _appSettings.Current.Stores.EverythingQuerySuffix = query.BuildQuery();

        // Window section
        _appSettings.Current.Window.NotificationDisplayDuration = NotificationDisplayDuration;
        _appSettings.Current.Window.BackdropStyle = WindowBackdropStyle;
        _appSettings.Current.Window.DateTimeFormat = DateTimeFormat;

        // Feature flags
        _appSettings.Current.FeatureFlags = FeatureFlags;
        IsResourceMonitorEnabled = FeatureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Resource Monitor
        _appSettings.Current.ResourceMonitor.CpuSmoothingIndex = CpuSmoothingIndex;
        _appSettings.Current.ResourceMonitor.RefreshRate = RefreshRate;

        // Miscellaneous
        _appSettings.Current.Github.Token
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
        var hash = _appSettings.Current.HotKey.GetHashCode();
        _appSettings.Current.HotKey = new HotKeySection(
            GetHotKey(),
            Key
        );

        List<bool> reboot =
        [
            hash != _appSettings.Current.HotKey.GetHashCode(),
            _infraSettings.Current.Database.DbPath != DbPath,
            _infraSettings.Current.Logging.MinimumLogLevel != SelectedLogLevel
        ];

        MapSettingsFromUiToDb();
        _infraSettings.Current.Logging.MinimumLogLevel = SelectedLogLevel;

        _infraSettings.Save();
        _appSettings.Save();

        var needRestart = reboot.Any(r => r);

        _logger.LogTrace("Saved settings. Need restart {NeedRestart}", needRestart);

        if (needRestart) { _hubService.GlobalNotifications.AskRestart(); }
    }

    #endregion
}