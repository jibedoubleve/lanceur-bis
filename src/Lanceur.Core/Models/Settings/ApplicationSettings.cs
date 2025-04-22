using Lanceur.Core.Constants;

namespace Lanceur.Core.Models.Settings;

public class ApplicationSettings
{
    #region Properties

    public string DbPath { get; set; } = Paths.DefaultDb;
    public TelemetrySection Telemetry { get; set; } = new();

    #endregion Properties
}