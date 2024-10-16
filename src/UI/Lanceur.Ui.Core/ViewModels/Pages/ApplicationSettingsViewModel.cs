using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Restart;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Services;
using Serilog.Core;
using Serilog.Events;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class ApplicationSettingsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _dbPath;
    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;

    private readonly IUserNotificationService _userNotificationService;
    private readonly IAppRestart _appRestart;
    [ObservableProperty] private ISettingsFacade _settings;
    private readonly IUserInteractionService _userInteraction;
    [ObservableProperty] private bool _showResults;
    [ObservableProperty] private  bool _isTraceEnabled;

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserNotificationService userNotificationService,
        IAppRestart appRestart,
        ISettingsFacade settings,
        LoggingLevelSwitch loggingLevelSwitch,
        IUserInteractionService userInteraction
    )
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(appRestart);

        _userNotificationService = userNotificationService;
        _appRestart = appRestart;
        _settings = settings;
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
        ShowResults = _settings.Application.Window.ShowResult;
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

    [RelayCommand]
    private async Task OnSaveSettingsAsync()
    {
        var hk = Settings.Application.HotKey;
        var hash = (hk.ModifierKey, hk.Key).GetHashCode();

        hk.ModifierKey = GetHotKey();
        hk.Key = Key;

        var reboot = hash != (hk.ModifierKey, hk.Key).GetHashCode();
        reboot &= Settings.Local.DbPath == DbPath;

        Settings.Local.DbPath = DbPath;
        Settings.Application.Window.ShowResult = ShowResults;

        Settings.Save();
        _userNotificationService.Success("Configuration saved.", "Saved");

        if (reboot)
        {
            const string msg = "Do you want to restart now to apply the new configuration?";
            if(await _userInteraction.AskUserYesNoAsync(msg)) _appRestart.Restart();
        }
    }

    #endregion
}