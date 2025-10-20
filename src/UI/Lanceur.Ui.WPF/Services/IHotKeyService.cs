using Lanceur.Core.Configuration.Sections;
using NHotkey;

namespace Lanceur.Ui.WPF.Services;

/// <summary>
///     Provides a service for managing global hotkeys,  
///     including registering handlers for hotkeys configured in the settings.  
/// </summary>
public interface IHotKeyService
{
    #region Methods

    /// <summary>
    ///     Registers an event handler for a specified hotkey.  
    ///     This method is typically used to override an existing hotkey if the previous registration failed.  
    /// </summary>
    /// <param name="handler">The event handler to invoke when the hotkey is pressed.</param>
    /// <param name="hotkey">The hotkey combination to register.</param>
    /// <returns><c>true</c> if the hotkey was successfully registered; otherwise, <c>false</c>.</returns>
    bool RegisterHandler(EventHandler<HotkeyEventArgs> handler, HotKeySection hotkey);

    /// <summary>
    /// Gets the hotkey configured in the settings
    /// </summary>
    HotKeySection HotKey { get; }
    #endregion
}



