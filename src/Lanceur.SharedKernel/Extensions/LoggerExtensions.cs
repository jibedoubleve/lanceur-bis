using System.Runtime.CompilerServices;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Extensions;

public static class LoggerExtensions
{
    #region Methods

    private static void WarnIfSlow(
        ILogger logger,
        Type source,
        string memberName,
        TimeSpan elapsed,
        double executionThreshold
    )
    {
        if (elapsed.TotalMilliseconds <= executionThreshold) { return; }

        logger.LogWarning(
            "Slow execution of {SourceFullName}.{CallerMemberName} in {ElapsedMilliseconds} milliseconds",
            source.FullName,
            memberName,
            elapsed.TotalMilliseconds
        );
    }

    /// <summary>
    ///     Measures the execution time of a code block and logs the duration using the provided logger.
    ///     If the execution time exceeds the specified threshold (default: 100 milliseconds), it logs a warning.
    /// </summary>
    /// <param name="logger">The logger to use for recording the execution time.</param>
    /// <param name="source">The type or context of the source to categorize the log entry.</param>
    /// <param name="executionThreshold">
    ///     The time threshold in milliseconds. If execution time exceeds this value, a warning is
    ///     logged.
    /// </param>
    /// <param name="callerMemberName">
    ///     The name of the method or property that called this method, used for more detailed
    ///     logging.
    /// </param>
    /// <returns>A <see cref="Measurement" /> object that logs the execution time when disposed.</returns>
    public static Measurement WarnIfSlow(
        this ILogger logger,
        object source,
        double executionThreshold = 100,
        [CallerMemberName] string callerMemberName = null
    )
    {
        ArgumentNullException.ThrowIfNull(logger);

        return source is null
            ? Measurement.Empty
            : TimeMeter.Measure(timespan =>
                WarnIfSlow(
                    logger,
                    source.GetType(),
                    callerMemberName,
                    timespan,
                    executionThreshold
                )
            );
    }

    #endregion
}