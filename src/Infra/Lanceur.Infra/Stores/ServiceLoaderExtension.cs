using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Infra.Stores;

public static class ServiceLoaderExtension
{
    #region Methods

    /// <summary>
    ///     Discovers and registers all macro into the dependency injection container.
    ///     Uses reflection to find classes decorated with <see cref="MacroAttribute" /> that implement
    ///     <see cref="MacroQueryResult" />.
    ///     All discovered services are registered as singleton instances.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register macros into.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMacroServices(this IServiceCollection serviceCollection)
    {
        var asm = Assembly.GetAssembly(typeof(GuidMacro));
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes<MacroAttribute>().Any())
                         .Where(t => t.IsAssignableTo(typeof(MacroQueryResult)))
                         .ToList();
        
        serviceCollection.AddTransient(sp => new Lazy<ISearchService>(sp.GetRequiredService<ISearchService>)); 
        foreach (var type in found)
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(MacroQueryResult), type));

        return serviceCollection;
    }

    /// <summary>
    ///     Discovers and registers all reserved aliases into the dependency injection container.
    ///     Uses reflection to find classes decorated with <see cref="ReservedAliasAttribute" /> that implement
    ///     <see cref="SelfExecutableQueryResult" />.
    ///     All discovered services are registered as singleton instances.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register reserved aliases into.</param>
    /// <param name="assemblyMarker">
    ///     A type belonging to the target assembly. Used as an anchor to locate
    ///     the assembly where reserved aliases are discovered.
    /// </param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddReservedAliasesServices(this IServiceCollection serviceCollection, Type assemblyMarker)
    {
        var asm = Assembly.GetAssembly(assemblyMarker);
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes<ReservedAliasAttribute>().Any())
                         .Where(t => t.IsAssignableTo(typeof(SelfExecutableQueryResult)))
                         .ToList();
        
        foreach (var type in found)
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(SelfExecutableQueryResult), type));
        
        return serviceCollection;
    }

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