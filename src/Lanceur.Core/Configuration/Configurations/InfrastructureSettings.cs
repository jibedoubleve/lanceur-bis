using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Constants;
using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Configuration.Configurations;

/// <summary>
///     Represents the configuration settings for the application.
/// </summary>
public class InfrastructureSettings
{
    #region Properties

    /// <summary>
    ///     Gets or sets the file system path to the SQLite database.
    ///     Defaults to <c>"%appdata%\probel\lanceur2\data.sqlite"</c>.
    /// </summary>
    public string DbPath { get; set; } = Paths.DefaultDb;

    /// <summary>
    ///     Gets or sets the minimum <see cref="LogLevel" /> to be recorded by the logging system.
    ///     Defaults to <see cref="Microsoft.Extensions.Logging.LogLevel.Information" />.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    ///     Gets or sets the telemetry configuration section.
    ///     Provides options for enabling and customizing telemetry collection.
    /// </summary>
    public TelemetrySection Telemetry { get; set; } = new();

    #endregion
}