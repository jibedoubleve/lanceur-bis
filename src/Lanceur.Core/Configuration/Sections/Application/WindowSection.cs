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
    ///     Gets or sets the position of the window when it is displayed on the screen.
    /// </summary>
    public PositionSection Position { get; set; } = new() { Left = double.MaxValue, Top = double.MaxValue };

    #endregion
}