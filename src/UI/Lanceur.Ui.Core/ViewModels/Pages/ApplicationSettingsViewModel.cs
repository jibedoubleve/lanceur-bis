using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
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

    [ObservableProperty] private string _bookmarkSourceBrowser;
    [ObservableProperty] private string _dbPath;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private  bool _isTraceEnabled;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    [ObservableProperty] private double _searchDelay;

    [ObservableProperty] private ISettingsFacade _settings;
    [ObservableProperty] private bool _showAtStartup;
    [ObservableProperty] private bool _showLastQuery;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private ObservableCollection<StoreShortcut> _storeShortcuts;
    private readonly IUserInteractionService _userInteraction;

    private readonly IUserNotificationService _userNotificationService;
    private readonly IViewFactory _viewFactory;
    [ObservableProperty] private string _windowBackdropStyle;

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
        IsTraceEnabled = loggingLevelSwitch.MinimumLevel == LogEventLevel.Verbose;

        // Miscellaneous
        DbPath = _settings.Local.DbPath;
        WindowBackdropStyle = _settings.Application.Window.BackdropStyle;
        SearchDelay = _settings.Application.SearchBox.SearchDelay;
        ShowResult = _settings.Application.SearchBox.ShowResult;
        ShowAtStartup = _settings.Application.SearchBox.ShowAtStartup;
        ShowLastQuery = _settings.Application.SearchBox.ShowLastQuery;
        BookmarkSourceBrowser = _settings.Application.Stores.BookmarkSourceBrowser;

        // Store overrides
        StoreShortcuts = new(_settings.Application.Stores.StoreOverrides);
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

    private void MapSettings()
    {
        Settings.Local.DbPath = DbPath;
        Settings.Application.Window.BackdropStyle = WindowBackdropStyle;
        Settings.Application.SearchBox.SearchDelay = SearchDelay;
        Settings.Application.SearchBox.ShowResult = ShowResult;
        Settings.Application.SearchBox.ShowAtStartup = ShowAtStartup;
        Settings.Application.SearchBox.ShowLastQuery = ShowLastQuery;
        Settings.Application.Stores.BookmarkSourceBrowser = BookmarkSourceBrowser;
        Settings.Application.Stores.StoreOverrides = StoreShortcuts.ToArray();
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
                                     .Replace("Store", "")
                                     .Replace("Lanceur.Infra.Stores.", "");

        var result = await _userInteraction.AskUserYesNoAsync(view, "Apply", "Cancel", $"Edit shortcut for store '{storeName}'");
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

        var reboot = hash != (hk.ModifierKey, hk.Key).GetHashCode();
        reboot &= Settings.Local.DbPath == DbPath;

        MapSettings();
        Settings.Save();
        _userNotificationService.Success("Configuration saved.", "Saved");

        if (reboot)
        {
            const string msg = "Do you want to restart now to apply the new configuration?";
            if (await _userInteraction.AskUserYesNoAsync(msg)) _appRestartService.Restart();
        }
    }

    #endregion
}