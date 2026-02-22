using Lanceur.Core.Configuration.Configurations;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Lanceur.Ui.Core.Extensions;

public static class InfrastructureSettingsExtension
{
    #region Methods

    public static LogEventLevel GetMinimumLogLevel(this InfrastructureSettings stg)
    {
        return stg.MinimumLogLevel switch
        {
            LogLevel.Trace       => LogEventLevel.Verbose,
            LogLevel.Debug       => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning     => LogEventLevel.Warning,
            LogLevel.Error       => LogEventLevel.Error,
            LogLevel.Critical    => LogEventLevel.Fatal,
            LogLevel.None        => LogEventLevel.Information,
            _                    => throw new ArgumentOutOfRangeException(
                $"The log level '{stg.MinimumLogLevel}' is not supported."
            )
        };
    }

    #endregion
}