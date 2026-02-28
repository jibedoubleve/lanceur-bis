using System.Diagnostics;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Logging;

/// <summary>
///     The use of this logger is NOT recommended. It's there
///     as a fallback if dev forgot to configure a logger.
///     This LogService is a very basic and NOT optimised logger.
///     It uses <see cref="System.Diagnostics.Trace" /> and also
///     reflection to log the name of the calling method.
/// </summary>
public class TraceLogger : ILogger
{
    #region Methods

    private static string GetCallerName()
    {
        var stackTrace = new StackTrace();
        var method = stackTrace!.GetFrame(1)!.GetMethod();
        return method!.Name;
    }

    private static void Write(Exception ex, string message, params object[] parameterValues)
    {
        var name = GetCallerName();

        Trace.WriteLine(
            $"[{name}] {message.Format(parameterValues)}{Environment.NewLine}{(ex is not null ? ex : string.Empty)}"
        );
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new TraceLoggerScope(state);

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter
    )
    {
        var parameters = new object[] { logLevel, eventId, state };
        var message = formatter(state, exception);
        Write(exception, message, parameters);
    }

    #endregion
}

/// <summary>
///     The use of this logger is NOT recommended. It's there
///     as a fallback if dev forgot to configure a logger.
///     This LogService is a very basic and NOT optimised logger.
///     It uses <see cref="System.Diagnostics.Trace" /> and also
///     reflection to log the name of the calling method.
/// </summary>
/// <remarks>
///     This is an implementation to allow getting logger with the
///     generics
/// </remarks>
public class TraceLogger<TSource> : TraceLogger, ILogger<TSource> { }