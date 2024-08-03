using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lanceur.Utils;

internal static class StaticLoggerFactory
{
    #region Methods

    private static ILogger GetLogger<TCategory>() => Locator.Current.GetService<ILoggerFactory>().GetLogger(typeof(TCategory));
    public static ILogger GetLogger<TCategory>(this IReadonlyDependencyResolver locator) => locator.GetService<ILoggerFactory>().GetLogger(typeof(TCategory));
    public static ILogger GetLogger(this QueryResult _) => GetLogger<QueryResult>();

    #endregion Methods
}

internal class DefaultLoggerFactory : ILoggerFactory
{
    #region Methods

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => new TraceLogger();

    public void Dispose() { }

    #endregion Methods
}