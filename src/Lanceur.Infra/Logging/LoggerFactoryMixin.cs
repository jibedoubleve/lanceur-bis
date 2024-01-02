using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lanceur.Infra.Logging;

public static class LoggerFactoryMixin
{
    #region Methods

    public static ILogger GetLogger(this ILoggerFactory loggerFactory, Type type)
    {
        var factory = loggerFactory ?? Locator.Current.GetService<ILoggerFactory>();
        return factory?.CreateLogger(type) ?? new TraceLogger();
    }

    public static ILogger<T> GetLogger<T>(this ILoggerFactory loggerFactory)
    {
        var factory = loggerFactory ?? Locator.Current.GetService<ILoggerFactory>();
        return factory?.CreateLogger<T>() ?? new TraceLogger<T>();
    }

    #endregion Methods
}