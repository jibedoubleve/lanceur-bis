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
    /// Gets or sets the style configuration for the window's backdrop.
    /// The default style is set to "Mica", which determines the window's visual appearance.
    /// </summary>
    public string BackdropStyle { get; set; } = "Mica";

    #endregion Properties
}