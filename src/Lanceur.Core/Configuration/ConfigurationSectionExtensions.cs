using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Core.Configuration;

public static class ServiceProviderExtensions
{
    #region Methods

    // TODO: this code could be removed and replace the Service Provider pattern by DI pattern
    public static ISection<T> GetSection<T>(this IServiceProvider serviceProvider)
        where T : class => serviceProvider.GetService<ISection<T>>();

    #endregion
}