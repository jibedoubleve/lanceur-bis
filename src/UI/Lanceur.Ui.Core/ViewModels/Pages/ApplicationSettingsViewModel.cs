using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores.Everything;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class ApplicationSettingsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _bookmarkSourceBrowser = string.Empty;
    [ObservableProperty] private string _dbPath = string.Empty;
    [ObservableProperty] private bool _excludeFilesInBinWithEverything;
    [ObservableProperty] private bool _excludeHiddenFilesWithEverything;
    [ObservableProperty] private bool _excludeSystemFilesWithEverything;
    private readonly IInteractionHub _hub;
    [ObservableProperty] private bool _includeOnlyExecFilesWithEverything;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private  bool _isTraceEnabled;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    private readonly LoggingLevelSwitch _loggingLevelSwitch;
    [ObservableProperty] private int _notificationDisplayDuration;
    [ObservableProperty] private double _searchDelay;
    [ObservableProperty] private ISettingsFacade _settings;
    [ObservableProperty] private bool _showAtStartup;
    [ObservableProperty] private bool _showLastQuery;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private ObservableCollection<StoreShortcut> _storeShortcuts = new();
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle = "Mica";

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IInteractionHub interactionHub,
        ILogger<ApplicationSettingsViewModel> logger,
        IAppRestartService appRestartService,
        ISettingsFacade settings,
        LoggingLevelSwitch loggingLevelSwitch,
        IViewFactory viewFactory
    )
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(appRestartService);

        _hub = interactionHub;
        _logger = logger;
        _settings = settings;
        _loggingLevelSwitch = loggingLevelSwitch;
        _viewFactory = viewFactory;

        // Hotkey
        var hk = _settings.Application.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        IsTraceEnabled = _loggingLevelSwitch.MinimumLevel == LogEventLevel.Verbose;

        // Miscellaneous
        MapSettingsFromDbToUi();

        PropertyChanged += (_, e) =>
        {
            _logger.LogTrace("Property '{Property}' changed", e.PropertyName);
            OnSaveSettings();
        };
    }

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
        DbPath = Settings.Local.DbPath;

        // Search box section
        SearchDelay = Settings.Application.SearchBox.SearchDelay;
        ShowResult = Settings.Application.SearchBox.ShowResult;
        ShowAtStartup = Settings.Application.SearchBox.ShowAtStartup;
        ShowLastQuery = Settings.Application.SearchBox.ShowLastQuery;

        // Store section
        BookmarkSourceBrowser = Settings.Application.Stores.BookmarkSourceBrowser;
        StoreShortcuts = new(Settings.Application.Stores.StoreOverrides);

        // -- Everything Store
        var adapter = new EverythingQueryAdapter(Settings.Application.Stores.EverythingQuerySuffix);
        ExcludeHiddenFilesWithEverything = adapter.IsHiddenFilesExcluded;
        ExcludeSystemFilesWithEverything = adapter.IsSystemFilesExcluded;
        IncludeOnlyExecFilesWithEverything = adapter.SelectOnlyExecutable;
        ExcludeFilesInBinWithEverything = adapter.IsFilesInTrashBinExcluded;

        // Window section
        NotificationDisplayDuration = Settings.Application.Window.NotificationDisplayDuration;
        WindowBackdropStyle = Settings.Application.Window.BackdropStyle;
    }

    private void MapSettingsFromUiToDb()
    {
        Settings.Local.DbPath = DbPath;

        // Search box section
        Settings.Application.SearchBox.SearchDelay = SearchDelay;
        Settings.Application.SearchBox.ShowResult = ShowResult;
        Settings.Application.SearchBox.ShowAtStartup = ShowAtStartup;
        Settings.Application.SearchBox.ShowLastQuery = ShowLastQuery;

        // Store section
        Settings.Application.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        Settings.Application.Stores.StoreOverrides = StoreShortcuts.ToArray();

        // -- Everything Store
        var query = new EverythingQueryBuilder();
        if (ExcludeHiddenFilesWithEverything) query.ExcludeHiddenFiles();
        if (ExcludeSystemFilesWithEverything) query.ExcludeSystemFiles();
        if (IncludeOnlyExecFilesWithEverything) query.OnlyExecFiles();
        if (ExcludeFilesInBinWithEverything) query.ExcludeFilesInBin();
        Settings.Application.Stores.EverythingQuerySuffix = query.ToString();

        // Window section
        Settings.Application.Window.NotificationDisplayDuration = NotificationDisplayDuration;
        Settings.Application.Window.BackdropStyle = WindowBackdropStyle;

        // Miscellaneous
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

        var result = await _hub.Interactions.AskUserYesNoAsync(
            view,
            "Apply",
            "Cancel",
            $"Edit shortcut for store '{storeName}'"
        );
        if (!result) return;

        _hub.Notifications.Success($"Modification has been done on {storeName}. Don't forget to save to apply changes", "Updated.");
    }

    [RelayCommand]
    private void OnSaveSettings()
    {
        var hk = Settings.Application.HotKey;
        var hash = (hk.ModifierKey, hk.Key).GetHashCode();

        hk.ModifierKey = GetHotKey();
        hk.Key = Key;

        List<bool> reboot = [hash != (hk.ModifierKey, hk.Key).GetHashCode(), Settings.Local.DbPath != DbPath];

        MapSettingsFromUiToDb();
        _loggingLevelSwitch.MinimumLevel = IsTraceEnabled ? LogEventLevel.Verbose : LogEventLevel.Information;
        Settings.Save();
        _hub.Notifications.Success("Configuration saved.", "Saved");

        if (reboot.Any(r => r)) _hub.GlobalNotifications.AskRestart();
    }

    #endregion
}