using Lanceur.Core.Services;
using Splat;

namespace Lanceur.Infra.Utils
{
    public static class LocatorMixin
    {
        #region Methods

        public static IAppLogger GetLogger<TCategory>(this IReadonlyDependencyResolver locator, IAppLoggerFactory factory = null)
        {
            return factory?.GetLogger<TCategory>()
                ?? locator?.GetService<IAppLoggerFactory>()?.GetLogger<TCategory>()
                ?? new TraceLogger();
        }
        public static IAppLogger GetLogger(this IReadonlyDependencyResolver locator, Type category, IAppLoggerFactory factory = null)
        {
            return factory?.GetLogger(category)
                ?? locator?.GetService<IAppLoggerFactory>()?.GetLogger(category)
                ?? new TraceLogger();
        }

        #endregion Methods
    }
}