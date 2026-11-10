using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.IoC;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class ApplicationSettingsViewModel : ObservableObject
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    private readonly IUserCommunicationService _hubService;

    private bool _isSettingSaveActivated = true;
    private readonly ILogger<ApplicationSettingsViewModel> _logger;
    private readonly ApplicationSettingsSynchroniser _settings;

    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public ApplicationSettingsViewModel(
        IUserCommunicationService userCommunicationService,
        ILogger<ApplicationSettingsViewModel> logger,
        IViewFactory viewFactory,
        ApplicationSettingsSynchroniser settings,
        IAliasRepository aliasRepository)
    {
        ArgumentNullException.ThrowIfNull(userCommunicationService);
        ArgumentNullException.ThrowIfNull(aliasRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(viewFactory);


        _hubService = userCommunicationService;
        _logger = logger;
        _viewFactory = viewFactory;
        _settings = settings;
        _aliasRepository = aliasRepository;

        // Hotkey
        var hk = settings.HotKey;
        IsCtrl = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Control);
        IsAlt = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Alt);
        IsWin = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Windows);
        IsShift = hk.ModifierKey.IsFlagSet((int)ModifierKeys.Shift);
        Key = hk.Key;

        // Logging
        SelectedLogLevel = settings.MinimumLogLevel;

        // Miscellaneous
        settings.MapFrom(this);
        var featureFlags = settings.FeatureFlags;
        IsResourceMonitorEnabled = featureFlags.IsFeatureFlagEnabled(Features.ResourceDisplay);

        // Setup behaviour on property changed
        foreach (var flag in settings.FeatureFlags)
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

    /// <summary>
    ///     Lists the special characters that <paramref name="storeShortcut" /> can use: those that are
    ///     neither an alias name nor the shortcut of another store. Its current shortcut is always kept.
    /// </summary>
    private IEnumerable<string> GetAvailableStoreShortcuts(StoreShortcut storeShortcut)
    {
        var usedByAliases = _aliasRepository.GetExistingAliases(CmdlineManager.SpecialCharacters, 0);
        var usedByStores = StoreShortcuts.Where(s => s != storeShortcut)
                                         .Select(s => s.AliasOverride);
        var used = usedByAliases.Concat(usedByStores)
                                .OfType<string>()
                                .ToHashSet();

        return CmdlineManager.SpecialCharacters
                             .Where(c => c == storeShortcut.AliasOverride || !used.Contains(c));
    }

    [RelayCommand]
    private async Task OnEditStoreShortcut(StoreShortcut? storeShortcut)
    {
        if (storeShortcut is null)
        {
            _logger.LogWarning("No store shortcut selected.");
            return;
        }

        var selector = new StoreShortcutSelectorViewModel(
            GetAvailableStoreShortcuts(storeShortcut),
            storeShortcut.AliasOverride
        );
        var view = _viewFactory.CreateView(selector);
        var storeName = storeShortcut.StoreType
                                     ?.Replace("Lanceur.Infra.Stores.", "")
                                     .Replace("Store", "");

        var result = await _hubService.Dialogues.AskUserYesNoAsync(
            view,
            ButtonLabels.Apply,
            ButtonLabels.Cancel,
            $"Edit shortcut for store '{storeName}'"
        );

        if (!result) { return; }

        var error = ValidateStoreShortcut(selector.SelectedShortcut);
        if (error is not null)
        {
            _hubService.Notifications.Warning(error);
            return;
        }

        storeShortcut.AliasOverride = selector.SelectedShortcut;
        SaveSettings();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        string[] properties = [nameof(IsResourceMonitorEnabled), nameof(IsAdminModeEnabled)];
        if (properties.Contains(e.PropertyName)) { return; }

        _logger.LogTrace("Property {Property} changed", e.PropertyName);
        if (_isSettingSaveActivated) { SaveSettings(); }
    }

    /// <summary>
    ///     Checks that <paramref name="shortcut" /> can be used as a store shortcut.
    /// </summary>
    /// <returns>The reason why the shortcut is rejected, or <c>null</c> if it is valid.</returns>
    private string? ValidateStoreShortcut(string? shortcut)
    {
        if (!CmdlineManager.IsSpecialCharacter(shortcut))
        {
            return $"The shortcut '{shortcut}' is invalid. A store shortcut must be a single special character.";
        }

        return _aliasRepository.GetExistingAliases([shortcut!], 0).Any()
            ? $"The shortcut '{shortcut}' is already used as the name of an alias."
            : null;
    }

    internal int GetHotKey()
    {
        var result = 0;
        if (IsCtrl) { result += (int)ModifierKeys.Control; }

        if (IsAlt) { result += (int)ModifierKeys.Alt; }

        if (IsWin) { result += (int)ModifierKeys.Windows; }

        if (IsShift) { result += (int)ModifierKeys.Shift; }

        return result;
    }

    internal void SaveSettings()
    {
        var needRestart = _settings.SaveSettings(this);

        if (needRestart) { _hubService.GlobalNotifications.AskRestart(); }
    }

    /// <summary>
    ///     Disables autosave until the returned scope is disposed.
    ///     Property changes made in the meantime are not saved.
    /// </summary>
    public IDisposable DisableAutosave() => BooleanScope.Create(
        () => _isSettingSaveActivated = false,
        () => _isSettingSaveActivated = true
    );

    #endregion
}