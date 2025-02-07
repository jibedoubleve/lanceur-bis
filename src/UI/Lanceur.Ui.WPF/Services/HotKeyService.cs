using System.Windows.Input;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.Logging;
using NHotkey;
using NHotkey.Wpf;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public class HotKeyService : IHotKeyService
{
    #region Fields

    private readonly IDatabaseConfigurationService _databaseConfigurationService;

    private readonly ILogger<HotKeyService> _logger;

    #endregion

    #region Constructors

    public HotKeyService(ILogger<HotKeyService> logger, IDatabaseConfigurationService databaseConfigurationService)
    {
        _logger = logger;
        _databaseConfigurationService = databaseConfigurationService;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public string GetHotkeyToString()
    {
        var hotKey = _databaseConfigurationService.Current.HotKey;
        return $"{(ModifierKeys)hotKey.ModifierKey} - {(Key)hotKey.Key}";
    }

    /// <inheritdoc />
    public bool RegisterHandler(string name, EventHandler<HotkeyEventArgs> handler, HotKeySection hotkey)
    {
        var key = (Key)hotkey.Key;
        var modifier = (ModifierKeys)hotkey.ModifierKey;
        _logger.LogInformation("Setup shortcut. (Key: {Key}, Modifier: {Modifier})", key, modifier);
        try
        {
            HotkeyManager.Current.AddOrReplace(
                name,
                key,
                modifier,
                handler
            );
            return true;
        }
        catch (HotkeyAlreadyRegisteredException ex)
        {
            //Default values
            _logger.LogWarning(
                ex,
                "Impossible to set shortcut. (Key: {Key}, Modifier: {Modifier})",
                key,
                modifier
            );
            return false;
        }
    }

    /// <inheritdoc />
    public bool RegisterHandler(string name, EventHandler<HotkeyEventArgs> handler)
    {
        var hotKey = _databaseConfigurationService.Current.HotKey;
        return RegisterHandler(name, handler, hotKey);
    }

    #endregion
}