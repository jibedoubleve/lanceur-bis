namespace Lanceur.Core.Configuration.Sections;

public class TelemetrySection
{
    #region Constructors

    public TelemetrySection()
    {
        IsClefEnabled = false;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether logging to file using Clef format is enabled.
    /// </summary>
    public bool IsClefEnabled { get; set; }

    #endregion
}