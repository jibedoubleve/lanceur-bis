using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Logging;

public static class LoggerMixin
{
    #region Methods

    public static IDisposable BeginSingleScope(this ILogger logger, string key, object value)
    {
        if (!key.StartsWith('@')) { key = $"@{key}"; }
        return new LogScope(logger).Add(key, value).BeginScope();
    }

    public static void LogActivate<TView>(this ILogger logger)
    {
        logger.LogTrace("Activating view {View}", typeof(TView));
    }

    #endregion Methods
}