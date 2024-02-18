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
        return source is not null
            ? TimeMeter.Measure(source, (template, args) => logger?.LogTrace(template, args), callerMemberName)
            : Measurement.Empty;
    }

    #endregion Methods
}