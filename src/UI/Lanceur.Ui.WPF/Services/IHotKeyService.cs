using Lanceur.Core.Models.Settings;
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
    ///     Registers an event handler for a hotkey that is configured in the application settings.  
    ///     When the specified hotkey is triggered, the associated handler is executed.  
    /// </summary>
    /// <param name="name">The unique identifier of the hotkey as defined in the settings.</param>
    /// <param name="handler">The event handler to invoke when the hotkey is pressed.</param>
    /// <returns><c>true</c> if the hotkey was successfully registered; otherwise, <c>false</c>.</returns>
    bool RegisterHandler(string name, EventHandler<HotkeyEventArgs> handler);

    /// <summary>
    ///     Registers an event handler for a specified hotkey.  
    ///     This method is typically used to override an existing hotkey if the previous registration failed.  
    /// </summary>
    /// <param name="name">The unique identifier for the hotkey.</param>
    /// <param name="handler">The event handler to invoke when the hotkey is pressed.</param>
    /// <param name="hotkey">The hotkey combination to register.</param>
    /// <returns><c>true</c> if the hotkey was successfully registered; otherwise, <c>false</c>.</returns>
    bool RegisterHandler(string name, EventHandler<HotkeyEventArgs> handler, HotKeySection hotkey);

    /// <summary>
    ///     Retrieves the string representation of the currently configured hotkey.  
    /// </summary>
    /// <returns>A string describing the hotkey combination.</returns>
    string GetHotkeyToString();

    #endregion
}



