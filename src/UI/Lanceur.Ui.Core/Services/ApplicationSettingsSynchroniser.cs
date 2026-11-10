using System.Collections.ObjectModel;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores.Everything;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.Services;

public class ApplicationSettingsSynchroniser
{
    #region Fields

    private readonly ISettingsProvider<ApplicationSettings> _appSettings;
    private readonly IEnigma _enigma;
    private readonly ISettingsProvider<InfrastructureSettings> _infraSettings;
    private readonly ILogger<ApplicationSettingsSynchroniser> _logger;
    private readonly IStoreShortcutService _storeShortcutService;

    #endregion

    #region Constructors

    public ApplicationSettingsSynchroniser(
        ISettingsProvider<ApplicationSettings> appSettings,
        ISettingsProvider<InfrastructureSettings> infraSettings,
        IStoreShortcutService storeShortcutService,
        IEnigma enigma,
        ILogger<ApplicationSettingsSynchroniser> logger)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(infraSettings);
        _appSettings = appSettings;
        _infraSettings = infraSettings;
        _storeShortcutService = storeShortcutService;
        _enigma = enigma;
        _logger = logger;

        FeatureFlags = _appSettings.Value.FeatureFlags.ToArray();
    }

    #endregion

    #region Properties

    public IReadOnlyCollection<FeatureFlag> FeatureFlags { get; private set; }

    public HotKeySection HotKey => _appSettings.Value.HotKey;

    public LogLevel MinimumLogLevel => _infraSettings.Value.Logging.MinimumLogLevel;

    #endregion

    #region Methods

    private void MapInto(ApplicationSettingsViewModel vm)
    {
        _infraSettings.Value.Database.DbPath = vm.DbPath;

        // Search box section
        _appSettings.Value.SearchBox.SearchDelay = vm.SearchDelay;
        _appSettings.Value.SearchBox.ShowResult = vm.ShowResult;
        _appSettings.Value.SearchBox.ShowAtStartup = vm.ShowAtStartup;
        _appSettings.Value.SearchBox.ShowLastQuery = vm.ShowLastQuery;
        _appSettings.Value.SearchBox.ToggleVisibility = vm.ToggleVisibility;

        _appSettings.Value.SearchBox.IsSettingsButtonEnabled = vm.IsSettingsButtonEnabled;
        _appSettings.Value.SearchBox.IsStatusBarAlwaysVisible = vm.IsStatusBarAlwaysVisible;

        // Store section
        _appSettings.Value.Stores.BookmarkSourceBrowser = vm.BookmarkSourceBrowser;
        _appSettings.Value.Stores.StoreShortcuts = vm.StoreShortcuts.ToArray();

        // Everything Store
        var query = new EverythingQueryBuilder();
        if (vm.ExcludeHiddenFilesWithEverything) { query.ExcludeHiddenFiles(); }

        if (vm.ExcludeSystemFilesWithEverything) { query.ExcludeSystemFiles(); }

        if (vm.IncludeOnlyExecFilesWithEverything) { query.OnlyExecFiles(); }

        if (vm.ExcludeFilesInBinWithEverything) { query.ExcludeFilesInBin(); }

        _appSettings.Value.Stores.EverythingQuerySuffix = query.BuildQuery();

        // Window section
        _appSettings.Value.Window.NotificationDisplayDuration = vm.NotificationDisplayDuration;
        _appSettings.Value.Window.BackdropStyle = vm.WindowBackdropStyle;
        _appSettings.Value.Window.DateTimeFormat = vm.DateTimeFormat;

        // Feature flags
        _appSettings.Value.FeatureFlags = FeatureFlags;
        vm.IsResourceMonitorEnabled = FeatureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Resource Monitor
        _appSettings.Value.ResourceMonitor.CpuSmoothingIndex = vm.CpuSmoothingIndex;
        _appSettings.Value.ResourceMonitor.RefreshRate = vm.RefreshRate;

        // Process launch
        _appSettings.Value.ProcessLaunch.EarlyExitCheckDelay = vm.EarlyExitCheckDelay;
        _appSettings.Value.ProcessLaunch.SlowStartupThreshold = vm.SlowStartupThreshold;
        _appSettings.Value.ProcessLaunch.StartupTimeout = vm.StartupTimeout;

        // Miscellaneous
        _appSettings.Value.Github.Token
            = vm.ApiToken.IsNullOrWhiteSpace() ? string.Empty : _enigma.Encrypt(vm.ApiToken);
    }

    public void MapFrom(ApplicationSettingsViewModel vm)
    {
        vm.DbPath = _infraSettings.Value.Database.DbPath;

        // Search box section
        vm.SearchDelay = _appSettings.Value.SearchBox.SearchDelay;
        vm.ShowResult = _appSettings.Value.SearchBox.ShowResult;
        vm.ShowAtStartup = _appSettings.Value.SearchBox.ShowAtStartup;
        vm.ShowLastQuery = _appSettings.Value.SearchBox.ShowLastQuery;
        vm.ToggleVisibility = _appSettings.Value.SearchBox.ToggleVisibility;

        vm.IsSettingsButtonEnabled = _appSettings.Value.SearchBox.IsSettingsButtonEnabled;
        vm.IsStatusBarAlwaysVisible = _appSettings.Value.SearchBox.IsStatusBarAlwaysVisible;

        // Store section
        vm.BookmarkSourceBrowser = _appSettings.Value.Stores.BookmarkSourceBrowser;

        var shortcuts = _storeShortcutService.Resolve(_appSettings.Value.Stores);
        vm.StoreShortcuts = new ObservableCollection<StoreShortcut>(shortcuts);

        // Everything Store
        var adapter = new EverythingQueryAdapter(_appSettings.Value.Stores.EverythingQuerySuffix);
        vm.ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        vm.ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        vm.IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        vm.ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        vm.NotificationDisplayDuration = _appSettings.Value.Window.NotificationDisplayDuration;
        vm.WindowBackdropStyle = _appSettings.Value.Window.BackdropStyle;
        vm.DateTimeFormat = _appSettings.Value.Window.DateTimeFormat;

        // Feature flags
        FeatureFlags = [.. _appSettings.Value.FeatureFlags];

        // Resource Monitor
        vm.CpuSmoothingIndex = _appSettings.Value.ResourceMonitor.CpuSmoothingIndex;
        vm.RefreshRate = _appSettings.Value.ResourceMonitor.RefreshRate;

        // Process launch
        vm.EarlyExitCheckDelay = _appSettings.Value.ProcessLaunch.EarlyExitCheckDelay;
        vm.SlowStartupThreshold = _appSettings.Value.ProcessLaunch.SlowStartupThreshold;
        vm.StartupTimeout = _appSettings.Value.ProcessLaunch.StartupTimeout;

        // Miscellaneous
        var token = _appSettings.Value.Github.Token;
        vm.ApiToken = token.IsNullOrWhiteSpace() ? string.Empty : _enigma.Decrypt(token!);
    }

    public bool SaveSettings(ApplicationSettingsViewModel vm)
    {
        var hash = _appSettings.Value.HotKey.GetHashCode();
        _appSettings.Value.HotKey = new HotKeySection(
            vm.GetHotKey(),
            vm.Key
        );

        List<bool> reboot =
        [
            hash != _appSettings.Value.HotKey.GetHashCode(),
            _infraSettings.Value.Database.DbPath != vm.DbPath,
            _infraSettings.Value.Logging.MinimumLogLevel != vm.SelectedLogLevel
        ];

        MapInto(vm);
        _infraSettings.Value.Logging.MinimumLogLevel = vm.SelectedLogLevel;

        _infraSettings.Save();
        _appSettings.Save();

        var needRestart = reboot.Any(r => r);

        _logger.LogTrace("Saved settings. Need restart {NeedRestart}", needRestart);

        return needRestart;
    }

    #endregion
}