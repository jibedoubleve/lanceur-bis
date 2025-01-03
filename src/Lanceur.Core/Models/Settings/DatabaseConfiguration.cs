namespace Lanceur.Core.Models.Settings;

/// <summary>
/// Represents the configuration settings for the application. 
/// This configuration is stored directly within the database.
/// </summary>
public class DatabaseConfiguration
{
    #region Properties

    /// <summary>
    /// Gets or sets the hotkey used to display the search window.
    /// </summary>
    public HotKeySection HotKey { get; set; } = HotKeySection.Default;

    /// <summary>
    /// Gets or sets the delay (in milliseconds) without keystrokes before triggering the search
    /// with the text that has already been typed. Acts as a "watchdog" to wait for typing pauses.
    /// </summary>
    public double SearchDelay { get; set; } = 50;

    /// <summary>
    /// Gets or sets the configuration settings related to the application's window.
    /// </summary>
    public WindowSection Window { get; set; } = WindowSection.Default;

    #endregion
}