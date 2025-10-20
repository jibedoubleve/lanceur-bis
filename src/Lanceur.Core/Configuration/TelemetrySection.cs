namespace Lanceur.Core.Configuration;

public class TelemetrySection
{
    #region Constructors

    public TelemetrySection()
    {
        IsClefEnabled = false;
        IsLokiEnabled = true;
        IsSeqEnabled = true;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether logging to file using Clef format is enabled.
    /// </summary>
    public bool IsClefEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether logging using Loki is enabled.
    /// </summary>
    public bool IsLokiEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether logging using Seq is enabled.
    /// </summary>
    public bool IsSeqEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the API key used for Seq logging.
    /// </summary>
    public string SeqApiKey { get; set; }

    #endregion
}