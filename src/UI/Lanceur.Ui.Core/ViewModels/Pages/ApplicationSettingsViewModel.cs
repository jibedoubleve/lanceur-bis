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

    private readonly ISettingsProvider<ApplicationSettings> _appSettings;

    private readonly IEnigma _enigma;

    private readonly IUserCommunicationService _hubService;

    private readonly ISettingsProvider<InfrastructureSettings> _infraSettings;

    private readonly ILogger<ApplicationSettingsViewModel> _logger;

    private readonly IStoreShortcutService _storeShortcutService;

    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserCommunicationService userCommunicationService,
        ILogger<ApplicationSettingsViewModel> logger,
        IViewFactory viewFactory,
        IEnigma enigma,
        IStoreShortcutService storeShortcutService,
        ISettingsProvider<ApplicationSettings> appSettings,
        ISettingsProvider<InfrastructureSettings> infraSettings)
    {
        ArgumentNullException.ThrowIfNull(userCommunicationService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(viewFactory);
        ArgumentNullException.ThrowIfNull(enigma);
        ArgumentNullException.ThrowIfNull(storeShortcutService);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(infraSettings);


        _hubService = userCommunicationService;
        _logger = logger;
        _viewFactory = viewFactory;
        _enigma = enigma;
        _storeShortcutService = storeShortcutService;
        _appSettings = appSettings;
        _infraSettings = infraSettings;

        // Hotkey
        var hk = _appSettings.Value.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        SelectedLogLevel = infraSettings.Value.Logging.MinimumLogLevel;

        // Miscellaneous
        MapSettingsFromDbToUi();
        var featureFlags = appSettings.Value.FeatureFlags.ToArray();
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

    [ObservableProperty]
    public partial string ApiToken { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BookmarkSourceBrowser { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CpuSmoothingIndex { get; set; }

    [ObservableProperty]
    public partial string DateTimeFormat { get; set; } = "";

    [ObservableProperty]
    public partial string DbPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int EarlyExitCheckDelay { get; set; }

    [ObservableProperty]
    public partial bool ExcludeFilesInBinWithEverything { get; set; }

    [ObservableProperty]
    public partial bool ExcludeHiddenFilesWithEverything { get; set; }

    [ObservableProperty]
    public partial bool ExcludeSystemFilesWithEverything { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FeatureFlag> FeatureFlags { get; set; } = [];

    [ObservableProperty]
    public partial bool IncludeOnlyExecFilesWithEverything { get; set; }

    [ObservableProperty]
    public partial bool IsAdminModeEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsAlt { get; set; }

    [ObservableProperty]
    public partial bool IsCtrl { get; set; }

    [ObservableProperty]
    public partial bool IsResourceMonitorEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsSettingsButtonEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsShift { get; set; }

    [ObservableProperty]
    public partial bool IsStatusBarAlwaysVisible { get; set; }

    [ObservableProperty]
    public partial bool IsWin { get; set; }

    [ObservableProperty]
    public partial int Key { get; set; }

    public static ObservableCollection<LogLevel> LogLevels => [LogLevel.Trace, LogLevel.Debug, LogLevel.Information];

    [ObservableProperty]
    public partial int NotificationDisplayDuration { get; set; }

    [ObservableProperty]
    public partial int RefreshRate { get; set; }

    [ObservableProperty]
    public partial double SearchDelay { get; set; }

    [ObservableProperty]
    public partial LogLevel SelectedLogLevel { get; set; }

    [ObservableProperty]
    public partial bool ShowAtStartup { get; set; }

    [ObservableProperty]
    public partial bool ShowLastQuery { get; set; }

    [ObservableProperty]
    public partial bool ShowResult { get; set; }

    [ObservableProperty]
    public partial int SlowStartupThreshold { get; set; }

    [ObservableProperty]
    public partial int StartupTimeout { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<StoreShortcut> StoreShortcuts { get; set; } = [];

    [ObservableProperty]
    public partial bool ToggleVisibility { get; set; }

    [ObservableProperty]
    public partial string WindowBackdropStyle { get; set; } = "Mica";

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
        DbPath = _infraSettings.Value.Database.DbPath;

        // Search box section
        SearchDelay = _appSettings.Value.SearchBox.SearchDelay;
        ShowResult = _appSettings.Value.SearchBox.ShowResult;
        ShowAtStartup = _appSettings.Value.SearchBox.ShowAtStartup;
        ShowLastQuery = _appSettings.Value.SearchBox.ShowLastQuery;
        ToggleVisibility = _appSettings.Value.SearchBox.ToggleVisibility;

        IsSettingsButtonEnabled = _appSettings.Value.SearchBox.IsSettingsButtonEnabled;
        IsStatusBarAlwaysVisible = _appSettings.Value.SearchBox.IsStatusBarAlwaysVisible;

        // Store section
        BookmarkSourceBrowser = _appSettings.Value.Stores.BookmarkSourceBrowser;

        var shortcuts = _storeShortcutService.Resolve(_appSettings.Value.Stores);
        StoreShortcuts = new ObservableCollection<StoreShortcut>(shortcuts);

        // Everything Store
        var adapter = new EverythingQueryAdapter(_appSettings.Value.Stores.EverythingQuerySuffix);
        ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        NotificationDisplayDuration = _appSettings.Value.Window.NotificationDisplayDuration;
        WindowBackdropStyle = _appSettings.Value.Window.BackdropStyle;
        DateTimeFormat = _appSettings.Value.Window.DateTimeFormat;

        // Feature flags
        FeatureFlags = [.. _appSettings.Value.FeatureFlags];

        // Resource Monitor
        CpuSmoothingIndex = _appSettings.Value.ResourceMonitor.CpuSmoothingIndex;
        RefreshRate = _appSettings.Value.ResourceMonitor.RefreshRate;

        // Process launch
        EarlyExitCheckDelay = _appSettings.Value.ProcessLaunch.EarlyExitCheckDelay;
        SlowStartupThreshold = _appSettings.Value.ProcessLaunch.SlowStartupThreshold;
        StartupTimeout = _appSettings.Value.ProcessLaunch.StartupTimeout;

        // Miscellaneous
        var token = _appSettings.Value.Github.Token;
        ApiToken = token.IsNullOrWhiteSpace() ? string.Empty : _enigma.Decrypt(token!);
    }

    private void MapSettingsFromUiToDb()
    {
        _infraSettings.Value.Database.DbPath = DbPath;

        // Search box section
        _appSettings.Value.SearchBox.SearchDelay = SearchDelay;
        _appSettings.Value.SearchBox.ShowResult = ShowResult;
        _appSettings.Value.SearchBox.ShowAtStartup = ShowAtStartup;
        _appSettings.Value.SearchBox.ShowLastQuery = ShowLastQuery;
        _appSettings.Value.SearchBox.ToggleVisibility = ToggleVisibility;

        _appSettings.Value.SearchBox.IsSettingsButtonEnabled = IsSettingsButtonEnabled;
        _appSettings.Value.SearchBox.IsStatusBarAlwaysVisible = IsStatusBarAlwaysVisible;

        // Store section
        _appSettings.Value.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        _appSettings.Value.Stores.StoreShortcuts = StoreShortcuts.ToArray();

        // Everything Store
        var query = new EverythingQueryBuilder();
        if (ExcludeHiddenFilesWithEverything) { query.ExcludeHiddenFiles(); }

        if (ExcludeSystemFilesWithEverything) { query.ExcludeSystemFiles(); }

        if (IncludeOnlyExecFilesWithEverything) { query.OnlyExecFiles(); }

        if (ExcludeFilesInBinWithEverything) { query.ExcludeFilesInBin(); }

        _appSettings.Value.Stores.EverythingQuerySuffix = query.BuildQuery();

        // Window section
        _appSettings.Value.Window.NotificationDisplayDuration = NotificationDisplayDuration;
        _appSettings.Value.Window.BackdropStyle = WindowBackdropStyle;
        _appSettings.Value.Window.DateTimeFormat = DateTimeFormat;

        // Feature flags
        _appSettings.Value.FeatureFlags = FeatureFlags;
        IsResourceMonitorEnabled = FeatureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Resource Monitor
        _appSettings.Value.ResourceMonitor.CpuSmoothingIndex = CpuSmoothingIndex;
        _appSettings.Value.ResourceMonitor.RefreshRate = RefreshRate;

        // Process launch
        _appSettings.Value.ProcessLaunch.EarlyExitCheckDelay = EarlyExitCheckDelay;
        _appSettings.Value.ProcessLaunch.SlowStartupThreshold = SlowStartupThreshold;
        _appSettings.Value.ProcessLaunch.StartupTimeout = StartupTimeout;

        // Miscellaneous
        _appSettings.Value.Github.Token
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
        var hash = _appSettings.Value.HotKey.GetHashCode();
        _appSettings.Value.HotKey = new HotKeySection(
            GetHotKey(),
            Key
        );

        List<bool> reboot =
        [
            hash != _appSettings.Value.HotKey.GetHashCode(),
            _infraSettings.Value.Database.DbPath != DbPath,
            _infraSettings.Value.Logging.MinimumLogLevel != SelectedLogLevel
        ];

        MapSettingsFromUiToDb();
        _infraSettings.Value.Logging.MinimumLogLevel = SelectedLogLevel;

        _infraSettings.Save();
        _appSettings.Save();

        var needRestart = reboot.Any(r => r);

        _logger.LogTrace("Saved settings. Need restart {NeedRestart}", needRestart);

        if (needRestart) { _hubService.GlobalNotifications.AskRestart(); }
    }

    #endregion
}