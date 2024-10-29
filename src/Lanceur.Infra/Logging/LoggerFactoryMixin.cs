using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Logging;

public static class LoggerFactoryMixin
{
    #region Methods

    public static ILogger<T> GetLogger<T>(this ILoggerFactory loggerFactory)
    {
        return loggerFactory?.CreateLogger<T>() ?? new TraceLogger<T>();
    }

    #endregion
}