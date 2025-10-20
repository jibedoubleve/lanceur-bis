using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores.Everything;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Constants;
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
    [ObservableProperty] private bool _excludeFilesInBinWithEverything;
    [ObservableProperty] private bool _excludeHiddenFilesWithEverything;
    [ObservableProperty] private bool _excludeSystemFilesWithEverything;
    [ObservableProperty] private ObservableCollection<FeatureFlag> _featureFlags = [];
    private readonly IInteractionHubService _hubService;
    [ObservableProperty] private bool _includeOnlyExecFilesWithEverything;
    [ObservableProperty] private bool _isAdminModeEnabled;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isResourceMonitorEnabled;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    [ObservableProperty] private int _notificationDisplayDuration;
    [ObservableProperty] private int _refreshRate;
    [ObservableProperty] private double _searchDelay;
    [ObservableProperty] private  LogLevel _selectedLogLevel;
    [ObservableProperty] private IConfigurationFacade _configuration;
    [ObservableProperty] private bool _showAtStartup;
    [ObservableProperty] private bool _showLastQuery;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private ObservableCollection<StoreShortcut> _storeShortcuts = [];
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle = "Mica";

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IInteractionHubService interactionHubService,
        ILogger<ApplicationSettingsViewModel> logger,
        IAppRestartService appRestartService,
        IConfigurationFacade configuration,
        IViewFactory viewFactory,
        IEnigma enigma
    )
    {
        ArgumentNullException.ThrowIfNull(interactionHubService);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(appRestartService);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(viewFactory);


        _hubService = interactionHubService;
        _logger = logger;
        _configuration = configuration;
        _viewFactory = viewFactory;
        _enigma = enigma;

        // Hotkey
        var hk = _configuration.Application.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        SelectedLogLevel = _configuration.Local.MinimumLogLevel; 

        // Miscellaneous
        MapSettingsFromDbToUi();
        IsResourceMonitorEnabled = Configuration.Application.FeatureFlags.Any(
            e => e.FeatureName.Equals(Features.ResourceDisplay, StringComparison.OrdinalIgnoreCase) && e.Enabled
        );

        // Setup behaviour on property changed
        foreach (var flag in FeatureFlags) flag.PropertyChanged += OnPropertyChanged;
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
        if (IsCtrl) result += (int)ModifierKeys.Control;
        if (IsAlt) result += (int)ModifierKeys.Alt;
        if (IsWin) result += (int)ModifierKeys.Windows;
        if (IsShift) result += (int)ModifierKeys.Shift;
        return result;
    }

    private void MapSettingsFromDbToUi()
    {
        DbPath = Configuration.Local.DbPath;

        // Search box section
        SearchDelay = Configuration.Application.SearchBox.SearchDelay;
        ShowResult = Configuration.Application.SearchBox.ShowResult;
        ShowAtStartup = Configuration.Application.SearchBox.ShowAtStartup;
        ShowLastQuery = Configuration.Application.SearchBox.ShowLastQuery;

        // Store section
        BookmarkSourceBrowser = Configuration.Application.Stores.BookmarkSourceBrowser;
        StoreShortcuts = new(Configuration.Application.Stores.StoreShortcuts);

        // Everything Store
        var adapter = new EverythingQueryAdapter(Configuration.Application.Stores.EverythingQuerySuffix);
        ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        NotificationDisplayDuration = Configuration.Application.Window.NotificationDisplayDuration;
        WindowBackdropStyle = Configuration.Application.Window.BackdropStyle;

        // Feature flags
        FeatureFlags = new(Configuration.Application.FeatureFlags);

        // Resource Monitor
        CpuSmoothingIndex = Configuration.Application.ResourceMonitor.CpuSmoothingIndex;
        RefreshRate = Configuration.Application.ResourceMonitor.RefreshRate;

        // Miscellaneous
        var token = Configuration.Application.Github.Token;
        ApiToken = token.IsNullOrWhiteSpace() ? string.Empty : _enigma.Decrypt(token);
    }

    private void MapSettingsFromUiToDb()
    {
        Configuration.Local.DbPath = DbPath;

        // Search box section
        Configuration.Application.SearchBox.SearchDelay = SearchDelay;
        Configuration.Application.SearchBox.ShowResult = ShowResult;
        Configuration.Application.SearchBox.ShowAtStartup = ShowAtStartup;
        Configuration.Application.SearchBox.ShowLastQuery = ShowLastQuery;

        // Store section
        Configuration.Application.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        Configuration.Application.Stores.StoreShortcuts = StoreShortcuts.ToArray();

        // Everything Store
        var query = new EverythingQueryBuilder();
        if (ExcludeHiddenFilesWithEverything) query.ExcludeHiddenFiles();
        if (ExcludeSystemFilesWithEverything) query.ExcludeSystemFiles();
        if (IncludeOnlyExecFilesWithEverything) query.OnlyExecFiles();
        if (ExcludeFilesInBinWithEverything) query.ExcludeFilesInBin();
        Configuration.Application.Stores.EverythingQuerySuffix = query.BuildQuery();

        // Window section
        Configuration.Application.Window.NotificationDisplayDuration = NotificationDisplayDuration;
        Configuration.Application.Window.BackdropStyle = WindowBackdropStyle;

        // Feature flags
        Configuration.Application.FeatureFlags = FeatureFlags;
        IsResourceMonitorEnabled = FeatureFlags.Any(
            e => e.FeatureName.Equals(Features.ResourceDisplay, StringComparison.OrdinalIgnoreCase) && e.Enabled
        );

        // Resource Monitor
        Configuration.Application.ResourceMonitor.CpuSmoothingIndex = CpuSmoothingIndex;
        Configuration.Application.ResourceMonitor.RefreshRate = RefreshRate;

        // Miscellaneous
        Configuration.Application.Github.Token = ApiToken.IsNullOrWhiteSpace() ? string.Empty : _enigma.Encrypt(ApiToken);
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
                                     .Replace("Lanceur.Infra.Stores.", "")
                                     .Replace("Store", "");

        var savedAliasOverride = storeShortcut.AliasOverride;
        var result = await _hubService.Interactions.AskUserYesNoAsync(
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
        if (properties.Contains(e.PropertyName)) return;

        _logger.LogTrace("Property {Property} changed", e.PropertyName);
        SaveSettings();
    }

    internal void SaveSettings()
    {
        var hk = Configuration.Application.HotKey;
        var hash = (hk.ModifierKey, hk.Key).GetHashCode();

        Configuration.Application.SetHotKey(GetHotKey(), Key);

        List<bool> reboot = [
            hash != (hk.ModifierKey, hk.Key).GetHashCode(), 
            Configuration.Local.DbPath != DbPath,
            Configuration.Local.MinimumLogLevel != SelectedLogLevel
        ];

        MapSettingsFromUiToDb();
        Configuration.Local.MinimumLogLevel = SelectedLogLevel;
        Configuration.Save();

        var needRestart = reboot.Any(r => r);
        
        _logger.LogTrace("Saved settings. Need restart {NeedRestart}", needRestart);
        
        if (needRestart) _hubService.GlobalNotifications.AskRestart();
    }

    #endregion
}