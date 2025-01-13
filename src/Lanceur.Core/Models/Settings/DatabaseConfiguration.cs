namespace Lanceur.Core.Models.Settings;

/// <summary>
///     Represents the configuration settings for the application.
///     This configuration is stored directly within the database.
/// </summary>
public class DatabaseConfiguration
{
    #region Properties

    /// <summary>
    ///     Gets or sets the hotkey used to display the search window.
    /// </summary>
    public HotKeySection HotKey { get; set; } = HotKeySection.Default;

    /// <summary>
    ///     Gets or sets the delay (in milliseconds) without keystrokes before triggering the search
    ///     with the text that has already been typed. Acts as a "watchdog" to wait for typing pauses.
    /// </summary>
    public double SearchDelay { get; set; } = 50;

    /// <summary>
    ///     Gets or sets a value indicating whether the window should be shown automatically at startup.
    ///     Default is <c>true</c>, meaning the window will be shown at application startup.
    /// </summary>
    public bool ShowAtStartup { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the last search query should be displayed when the user opens the search
    ///     box.
    ///     When set to <c>true</c>, the search box will display the last query. If set to <c>false</c>, the search box will be
    ///     empty.
    /// </summary>
    public bool ShowLastQuery { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether search results are shown immediately when the search window is displayed.
    ///     If set to <c>true</c>, all results are displayed immediately, which may impact performance.
    ///     If set to <c>false</c>, no results are shown until a search is executed.
    /// </summary>
    public bool ShowResult { get; set; }

    /// <summary>
    ///     Gets or sets the configuration settings related to the application's window.
    /// </summary>
    public WindowSection Window { get; set; } = WindowSection.Default;

    #endregion
}