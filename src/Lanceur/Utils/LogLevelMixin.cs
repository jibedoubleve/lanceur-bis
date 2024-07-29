using System;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Lanceur.Utils;

internal static class LogLevelMixin
{
    public static LogEventLevel ToSerilogLogLevel(this LogLevel level) => level switch
    {
        LogLevel.Trace       => LogEventLevel.Verbose,
        LogLevel.Debug       => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning     => LogEventLevel.Warning,
        LogLevel.Error       => LogEventLevel.Error,
        LogLevel.Critical    => LogEventLevel.Fatal,
        LogLevel.None        => LogEventLevel.Information,
        _                    => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    public static LogLevel ToLogLevel(this LogEventLevel level) => level switch 
    {
        LogEventLevel.Verbose     => LogLevel.Trace,
        LogEventLevel.Debug       => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Warning     => LogLevel.Warning,
        LogEventLevel.Error       => LogLevel.Error,
        LogEventLevel.Fatal       => LogLevel.Critical,
        _                         => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };
}