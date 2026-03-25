using System.Globalization;

namespace Lanceur.Core.Configuration.Sections.Application;

public sealed class WindowSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the style configuration for the window's backdrop.
    ///     The default style is set to "Mica", which determines the window's visual appearance.
    /// </summary>
    public string BackdropStyle { get; set; } = "Mica";

    /// <summary>
    ///     Gets or sets the format string used to display dates in the UI.
    ///     Accepts any format string valid for <see cref="DateTime.ToString(string)" />,
    ///     such as <c>"dd/MM/yyyy"</c> or <c>"MM-dd-yyyy"</c>.
    /// </summary>
    public string DateTimeFormat { get; set; } =
        $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} {DateTimeFormatInfo.CurrentInfo.ShortTimePattern}";

    /// <summary>
    ///     Gets or sets the duration (in seconds) that the notification remains visible in the UI before disappearing.
    /// </summary>
    public int NotificationDisplayDuration { get; set; } = 10;

    /// <summary>
    ///     Gets or sets the last known position of the window on the screen.
    ///     Defaults to <see cref="double.MaxValue" /> for both axes, which indicates that no position
    ///     has been saved yet and the window should be placed at its default position.
    /// </summary>
    /// <remarks>
    ///     The setter is required for JSON deserialisation. Use <see cref="WindowSectionExtensions.SetPosition" />
    ///     to update the position at runtime.
    /// </remarks>
    public PositionSection Position { get; set; } = new() { Left = double.MaxValue, Top = double.MaxValue };

    #endregion
}

public static class WindowSectionExtensions
{
    #region Methods

    public static void SetPosition(this WindowSection section, double left, double top)
    {
        section.Position.Left = left;
        section.Position.Top = top;
    }

    #endregion
}