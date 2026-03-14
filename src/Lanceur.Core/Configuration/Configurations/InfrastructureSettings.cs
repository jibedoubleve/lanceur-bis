using System.Diagnostics;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Configuration.Configurations;

/// <summary>
///     Represents the infrastructure-level configuration settings (database, logging, pipelines).
/// </summary>
public class InfrastructureSettings
{
    #region Properties

    /// <summary>
    ///     Migration-only setter: forwards the legacy flat <c>DbPath</c> value found in older JSON configs
    ///     to <see cref="Database" />.<see cref="DatabaseSection.DbPath" />.
    ///     The property is setter-only so Newtonsoft.Json never writes it back;
    ///     subsequent saves use the nested <c>Database.DbPath</c> location exclusively.
    /// </summary>
    [Obsolete("Use Database.DbPath. This property exists only for JSON migration.")]
    public string? DbPath
    {
        set { if (!string.IsNullOrEmpty(value)) Database.DbPath = value; }
    }

    /// <summary>
    ///     Gets or sets the database configuration (connection path, etc.).
    /// </summary>
    public DatabaseSection Database { get; set; } = new();

    /// <summary>
    ///     Gets or sets the logging configuration (minimum log level, etc.).
    /// </summary>
    public LoggingSection Logging { get; set; } = new();
    
    /// <summary>
    ///     Gets or sets the thumbnail pipeline configuration.
    ///     Provides options to control the behaviour of the pipeline channel and the number of concurrent consumers.
    /// </summary>
    public ThumbnailPipelineSection ThumbnailPipeline { get; set; } = new();

    #endregion
}