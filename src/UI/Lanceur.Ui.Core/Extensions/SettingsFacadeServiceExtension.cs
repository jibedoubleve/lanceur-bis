using Lanceur.Infra.Repositories;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Lanceur.Ui.Core.Extensions;

public static class SettingsFacadeServiceExtension
{
    #region Methods

    public static LogEventLevel GetMinimumLogLevel(this SettingsFacadeService src)
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
            _                    => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion
}