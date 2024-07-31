using System;
using System.Reactive.Linq;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lanceur.Utils;

public static class ObservableLoggingOverrideMixin
{
    #region Fields

    private static readonly ILogger Logger = Locator.Current.GetService<ILoggerFactory>()?.GetLogger(typeof(ObservableLoggingOverrideMixin));

    #endregion Fields

    #region Methods

    private static void WriteLog(LogLevel logLevel, string message, params object[] arguments)
    {
        if (Logger is null) return;
        
        switch (logLevel)
        {
            case LogLevel.Trace:
                Logger.LogTrace(message, arguments);
                break;

            case LogLevel.Debug:
                Logger.LogDebug(message, arguments);
                break;

            case LogLevel.Information:
            case LogLevel.None:
                Logger.LogInformation(message, arguments);
                break;

            case LogLevel.Warning:
                Logger.LogWarning(message, arguments);
                break;

            case LogLevel.Error:
                Logger.LogError(message, arguments);
                break;

            case LogLevel.Critical:
                Logger.LogCritical(message, arguments);
                break;

            default:
                Logger.LogInformation(message, arguments);
                break;
        }
    }

    /// <summary>
    /// Logs an Observable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="this">The source observable to log to splat.</param>
    /// <param name="message">An optional method.</param>
    /// <param name="stringifier">An optional Func to convert Ts to strings.</param>
    /// <param name="logLevel">The loglevel to apply</param>
    /// <returns>The same Observable.</returns>
    public static IObservable<T> WriteLog<T>(
        this IObservable<T> @this,
        string message,
        Func<T, string> stringifier = null,
        LogLevel logLevel = LogLevel.Trace)
    {
        if (stringifier is not null)
        {
            return @this.Do(
                x => WriteLog(logLevel, "{0} OnNext: {1}", message, stringifier(x)),
                ex => Logger.LogWarning(ex, "{Message}. OnError", message),
                () => WriteLog(logLevel, "{0} OnCompleted", message));
        }

        return @this.Do(
            x => WriteLog(logLevel, "{0} OnNext: {1}", message, x),
            ex => Logger.LogWarning(ex, "{Message}  OnError", message),
            () => WriteLog(logLevel, "{0} OnCompleted", message));
    }

    #endregion Methods
}