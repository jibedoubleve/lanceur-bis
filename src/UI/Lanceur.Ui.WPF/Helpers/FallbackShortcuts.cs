using System.Windows.Input;

namespace Lanceur.Ui.WPF.Helpers;

public record GlobalShortcut(Key Key, ModifierKeys ModifierKeys);

/// <summary>
/// A record that manages fallback shortcuts for the application.
/// It provides a list of pre-defined global shortcuts. If the current shortcut is unavailable,
/// the <c>Next()</c> method returns the next available shortcut in the list.
/// If all shortcuts are exhausted, an error is expected to be thrown.
/// </summary>
public record FallbackShortcuts
{
    /// <summary>
    /// List of global shortcuts to be used as fallbacks.
    /// Each shortcut is defined with a key and modifier keys.
    /// </summary>
    private readonly GlobalShortcut[] _globalShortcut = [
        new(Key.R, ModifierKeys.Control | ModifierKeys.Windows),
        new(Key.R, ModifierKeys.Alt | ModifierKeys.Shift), 
    ];

    private int _current;
    
    /// <summary>
    /// Indicates whether there are more shortcuts available in the list.
    /// </summary>
    public bool CanNext =>  _current <= _globalShortcut.Length;
    
    /// <summary>
    /// Returns the next available shortcut in the list.
    /// Throws an exception if no more shortcuts are available.
    /// </summary>
    /// <returns>The next <see cref="GlobalShortcut"/> in the list.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when attempting to access a shortcut beyond the list's bounds.</exception>
    public GlobalShortcut Next() => _globalShortcut[_current++];
}