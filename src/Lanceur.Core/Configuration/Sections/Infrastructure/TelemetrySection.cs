namespace Lanceur.Core.Configuration.Sections.Infrastructure;

public class TelemetrySection
{
    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether logging to file using Clef format is enabled.
    /// </summary>
    public bool IsClefEnabled { get; set; } = false;

    #endregion
}