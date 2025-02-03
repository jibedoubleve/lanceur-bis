namespace Lanceur.Core.Models.Settings;

public class WindowSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the style configuration for the window's backdrop.
    ///     The default style is set to "Mica", which determines the window's visual appearance.
    /// </summary>
    public string BackdropStyle { get; set; } = "Mica";

    /// <summary>
    ///     Gets the default window configuration, with default position settings.
    /// </summary>
    public static WindowSection Default => new() { Position = PositionSection.Default };

    /// <summary>
    ///     Gets or sets the duration (in seconds) that the notification remains visible in the UI before disappearing.
    /// </summary>
    public int NotificationDisplayDuration { get; set; } = 10;

    /// <summary>
    ///     Gets or sets the position of the window when it is displayed on the screen.
    /// </summary>
    public PositionSection Position { get; set; } = new();

    #endregion
}