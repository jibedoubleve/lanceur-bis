using System.Runtime.CompilerServices;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Mixins;

public static class LoggerMixin
{
    #region Methods

    public static Measurement MeasureExecutionTime(
        this ILogger logger,
        object source,
        [CallerMemberName] string callerMemberName = null)
    {
        return source is null
            ? Measurement.Empty
            : TimeMeter.Measure((timespan, message) =>
                                    LogTime(logger, source.GetType(), callerMemberName, timespan, message));
    }

    private static void LogTime(ILogger logger, Type source, string memberName, TimeSpan elapsed, string message)
    {
        if (elapsed.TotalMilliseconds > 100)
        {
            if (string.IsNullOrEmpty(message))
                logger.LogWarning(
                    "Slow execution of {SourceFullName}.{CallerMemberName} in {ElapsedMilliseconds} milliseconds",
                    source.FullName, memberName, elapsed.TotalMilliseconds);
            else
                logger.LogWarning(
                    "Slow execution of {SourceFullName}.{CallerMemberName} in {ElapsedMilliseconds} milliseconds. ['{Message}']",
                    source.FullName, memberName, elapsed.TotalMilliseconds, message);
        }
    }

    #endregion Methods
}