namespace Lanceur.Core.Models.Settings;

public class WindowSection
{
    #region Properties

    public static WindowSection Default => new() { Position = PositionSection.Default };
    public PositionSection Position { get; set; } = new();
    public bool ShowAtStartup { get; set; } = true;
    public bool ShowResult { get; set; }

    #endregion Properties
}