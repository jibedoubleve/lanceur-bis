using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Configuration.Sections.Infrastructure;

public sealed class LoggingSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether logging to file using Clef format is enabled.
    /// </summary>
    public bool IsClefEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets the minimum <see cref="LogLevel" /> to be recorded by the logging system.
    ///     Defaults to <see cref="Microsoft.Extensions.Logging.LogLevel.Information" />.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    #endregion
}