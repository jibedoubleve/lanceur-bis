using System.Windows.Input;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Microsoft.Extensions.Logging;
using NHotkey;
using NHotkey.Wpf;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public class HotKeyService : IHotKeyService
{
    #region Fields

    private readonly ISection<HotKeySection> _hotkeySection;


    private readonly ILogger<HotKeyService> _logger;

    #endregion

    #region Constructors

    public HotKeyService(ILogger<HotKeyService> logger, ISection<HotKeySection> hotkeySection)
    {
        _logger = logger;
        _hotkeySection = hotkeySection;
    }

    #endregion

    #region Properties

    public HotKeySection HotKey => _hotkeySection.Value;

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool RegisterHandler(EventHandler<HotkeyEventArgs> handler, HotKeySection hotkey)
    {
        var key = (Key)hotkey.Key;
        var modifier = (ModifierKeys)hotkey.ModifierKey;
        _logger.LogInformation("Setup shortcut. (Key: {Key}, Modifier: {Modifier})", key, modifier);
        try
        {
            HotkeyManager.Current.AddOrReplace(
                hotkey.ToString(),
                key,
                modifier,
                handler
            );
            return true;
        }
        catch (HotkeyAlreadyRegisteredException ex)
        {
            _logger.LogWarning(
                ex,
                "Impossible to set shortcut. (Key: {Key}, Modifier: {Modifier})",
                key,
                modifier
            );
            return false;
        }
    }

    #endregion
}