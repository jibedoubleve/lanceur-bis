namespace Lanceur.Core.Models.Settings;

public class WindowSection
{
    #region Properties

    /// <summary>
    /// Gets the default window configuration, with default position settings.
    /// </summary>
    public static WindowSection Default => new() { Position = PositionSection.Default };

    /// <summary>
    /// Gets or sets the position of the window when it is displayed on the screen.
    /// </summary>
    public PositionSection Position { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the window should be shown automatically at startup.
    /// Default is <c>true</c>, meaning the window will be shown at application startup.
    /// </summary>
    public bool ShowAtStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether search results are shown immediately when the search window is displayed.
    /// If set to <c>true</c>, all results are displayed immediately, which may impact performance.
    /// If set to <c>false</c>, no results are shown until a search is executed.
    /// </summary>
    public bool ShowResult { get; set; }

    /// <summary>
    /// Gets or sets the style configuration for the window's backdrop.
    /// The default style is set to "Mica", which determines the window's visual appearance.
    /// </summary>
    public string BackdropStyle { get; set; } = "Mica";

    #endregion Properties
}