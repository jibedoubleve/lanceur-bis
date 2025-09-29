using System.Runtime.CompilerServices;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Utils;

public static class TimeMeter
{
    #region Methods

    public static Measurement Measure(Action<TimeSpan> log) => new(log, null);

    public static Measurement Measure(Type caller, ILogger logger, [CallerMemberName] string callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(logger);

        return new(LogStart, LogSplit);

        void Log(TimeSpan ts, string type)
        {
            var template = "[{Source}] " + type + ": {Elapsed} sec.";

            logger.LogDebug(
                template,
                $"{caller.FullName}.{callerMemberName ?? "<Unknown>"}",
               Math.Round(ts.TotalMilliseconds, 3)
            );
        }

        void LogStart(TimeSpan ts) => Log(ts, "Elapsed");
        void LogSplit(TimeSpan ts) => Log(ts, "Split");
    }

    public static Measurement Measure(object @this, ILogger logger, [CallerMemberName] string callerMemberName = null)
        => Measure(@this.GetType(), logger, callerMemberName);

    #endregion
}