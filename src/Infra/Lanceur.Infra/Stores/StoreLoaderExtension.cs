using System.Reflection;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Infra.Stores;

public static class StoreLoaderExtension
{
    #region Methods

    /// <summary>
    ///     Discovers and registers all store services into the dependency injection container.
    ///     Uses reflection to find classes decorated with <see cref="StoreAttribute" /> that implement
    ///     <see cref="IStoreService" />.
    ///     All discovered services are registered as singleton instances.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register stores into.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddStoreServices(this IServiceCollection serviceCollection)
    {
        var asm = Assembly.GetAssembly(typeof(SearchService));
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes<StoreAttribute>().Any())
                         .Where(t => t.IsAssignableTo(typeof(IStoreService)))
                         .ToList();

        foreach (var type in found)
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IStoreService), type));

        return serviceCollection;
    }

    #endregion
}