using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Logging;

//TODO: analyse, it should probably be removed.
public static class LoggerFactoryExtensions
{
    #region Methods

    public static ILogger<T> GetLogger<T>(this ILoggerFactory loggerFactory)
        => loggerFactory?.CreateLogger<T>() ?? new TraceLogger<T>();

    #endregion
}