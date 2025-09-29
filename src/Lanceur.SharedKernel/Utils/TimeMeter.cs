using System.Runtime.CompilerServices;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Utils;

public static class TimeMeter
{
    #region Methods

    public static Measurement Measure(Action<TimeSpan> log) => new(log);

    public static Measurement Measure(object @this, ILogger logger, [CallerMemberName] string callerMemberName = null)
    {
        return new(Log);

        void Log(TimeSpan ts) => logger.LogDebug(
            "[{Type}.{SourceMethod}] Elapsed: {Elapsed} sec.",
            @this.GetType().FullName,
            callerMemberName,
            ts.ToStringInSeconds()
        );
    }

    public static Measurement MeasureTrace([CallerMemberName] string callerMemberName = null)
    {
        return new(Trace);

        void Trace(TimeSpan ts) => System.Diagnostics.Trace.WriteLine($"==========> [{callerMemberName}] Elapsed: {ts.ToStringInSeconds()} sec.");
    }
    #endregion
}