using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Lanceur.Ui.Core.Extensions;

public static class SettingsFacadeServiceExtension
{
    #region Methods

    public static LogEventLevel GetMinimumLogLevel(this ISettingsFacade src)
    {
        return src.Local.MinimumLogLevel switch
        {
            LogLevel.Trace       => LogEventLevel.Verbose,
            LogLevel.Debug       => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning     => LogEventLevel.Warning,
            LogLevel.Error       => LogEventLevel.Error,
            LogLevel.Critical    => LogEventLevel.Fatal,
            LogLevel.None        => LogEventLevel.Information,
            _                    => throw new ArgumentOutOfRangeException(
                $"The log level '{src.Local.MinimumLogLevel}' is not supported."
            )
        };
    }

    #endregion
}