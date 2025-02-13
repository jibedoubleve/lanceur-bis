using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Stores.Everything;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class ApplicationSettingsViewModel : ObservableObject
{
    #region Fields

    private readonly IAppRestartService _appRestartService;

    [ObservableProperty] private string _bookmarkSourceBrowser = string.Empty;
    [ObservableProperty] private string _dbPath = string.Empty;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private  bool _isTraceEnabled;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    [ObservableProperty] private int _notificationDisplayDuration;
    [ObservableProperty] private double _searchDelay;

    [ObservableProperty] private ISettingsFacade _settings;
    private readonly LoggingLevelSwitch _loggingLevelSwitch;
    [ObservableProperty] private bool _showAtStartup;
    [ObservableProperty] private bool _showLastQuery;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private ObservableCollection<StoreShortcut> _storeShortcuts = new();
    [ObservableProperty] private bool _excludeHiddenFilesWithEverything;
    [ObservableProperty] private bool _excludeSystemFilesWithEverything;
    [ObservableProperty] private bool _excludeFilesInBinWithEverything;
    [ObservableProperty] private bool _includeOnlyExecFilesWithEverything;
    private readonly IUserInteractionService _userInteraction;

    private readonly IUserNotificationService _userNotificationService;
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle = "Mica";

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserNotificationService userNotificationService,
        ILogger<ApplicationSettingsViewModel> logger,
        IAppRestartService appRestartService,
        ISettingsFacade settings,
        LoggingLevelSwitch loggingLevelSwitch,
        IViewFactory viewFactory,
        IUserInteractionService userInteraction
    )
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(appRestartService);

        _userNotificationService = userNotificationService;
        _logger = logger;
        _appRestartService = appRestartService;
        _settings = settings;
        _loggingLevelSwitch = loggingLevelSwitch;
        _viewFactory = viewFactory;
        _userInteraction = userInteraction;

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

        // Store overrides
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

        var result = await _userInteraction.AskUserYesNoAsync(
            view,
            "Apply",
            "Cancel",
            $"Edit shortcut for store '{storeName}'"
        );
        if (!result) return;

        _userNotificationService.Success($"Modification has been done on {storeName}. Don't forget to save to apply changes", "Updated.");
    }

    [RelayCommand]
    private async Task OnSaveSettingsAsync()
    {
        var hk = Settings.Application.HotKey;
        var hash = (hk.ModifierKey, hk.Key).GetHashCode();

        hk.ModifierKey = GetHotKey();
        hk.Key = Key;

        List<bool> reboot = [hash != (hk.ModifierKey, hk.Key).GetHashCode(), Settings.Local.DbPath != DbPath];

        MapSettingsFromUiToDb();
        _loggingLevelSwitch.MinimumLevel = IsTraceEnabled ? LogEventLevel.Verbose : LogEventLevel.Information;
        Settings.Save();
        _userNotificationService.Success("Configuration saved.", "Saved");

        if (reboot.Any(r => r))
        {
            const string msg = "Do you want to restart now to apply the new configuration?";
            if (await _userInteraction.AskUserYesNoAsync(msg)) _appRestartService.Restart();
        }
    }

    #endregion
}