namespace Lanceur.Core.Configuration.Sections;

public class SearchBoxSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the delay (in milliseconds) without keystrokes before triggering the search
    ///     with the text that has already been typed. Acts as a "watchdog" to wait for typing pauses.
    /// </summary>
    /// <remarks> Default value is <c>50</c></remarks>
    public double SearchDelay { get; set; } = 50;

    /// <summary>
    ///     Gets or sets a value indicating whether the window should be shown automatically at startup.
    ///     Default is <c>true</c>, meaning the window will be shown at application startup.
    /// </summary>
    /// <remarks> Default value is <c>True</c></remarks>
    public bool ShowAtStartup { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the last search query should be displayed when the user opens the search
    ///     box.
    ///     When set to <c>true</c>, the search box will display the last query. If set to <c>false</c>, the search box will be
    ///     empty.
    /// </summary>
    /// <remarks> Default value is <c>True</c></remarks>
    public bool ShowLastQuery { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether search results are shown immediately when the search window is displayed.
    ///     If set to <c>true</c>, all results are displayed immediately, which may impact performance.
    ///     If set to <c>false</c>, no results are shown until a search is executed.
    /// </summary>
    /// <remarks> Default value is <c>False</c></remarks>
    public bool ShowResult { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the shortcut toggles the search box,
    ///     showing it on first use and hiding it on subsequent use.
    /// </summary>
    /// <remarks> Default value is <c>True</c></remarks>
    public bool ToggleVisibility { get; set; } = true;

    #endregion
}