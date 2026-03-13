using Serilog.Events;

namespace Lanceur.Ui.Core.Utils;

public static class LogLevelUtil
{
    #region Methods

    /// <summary>
    ///     Returns the effective log level based on the command-line arguments.
    ///     <c>--verbose</c> takes precedence over <c>--debug</c>.
    /// </summary>
    /// <param name="fallbackLevel">
    ///     The log level to use when neither <c>--verbose</c> nor <c>--debug</c> is specified.
    ///     Defaults to <see cref="LogEventLevel.Debug" />.
    /// </param>
    /// <returns>
    ///     <see cref="LogEventLevel.Verbose" /> if <c>--verbose</c> is found;
    ///     <see cref="LogEventLevel.Debug" /> if <c>--debug</c> is found;
    ///     otherwise <paramref name="fallbackLevel" />.
    /// </returns>
    public static LogEventLevel GetLevel(LogEventLevel fallbackLevel = LogEventLevel.Debug)
    {
        var args = Environment.GetCommandLineArgs();
        
        if (args.Any(x => x.Contains("--verbose"))) { return LogEventLevel.Verbose; }

        if (args.Any(x => x.Contains("--debug"))) { return LogEventLevel.Debug; }

        return fallbackLevel;
    }

    #endregion
}