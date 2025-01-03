using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Logging;

//TODO: analyse, it should probably be removed.
public static class LoggerFactoryExtensions
{
    #region Methods

    public static ILogger<T> GetLogger<T>(this ILoggerFactory loggerFactory)
    {
        return loggerFactory?.CreateLogger<T>() ?? new TraceLogger<T>();
    }

    #endregion
}