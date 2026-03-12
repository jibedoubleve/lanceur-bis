using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Infra.Macros;

public static class MacroRegistrationExtension
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
    public static IServiceCollection AddMacros(this IServiceCollection serviceCollection)
    {
        var asm = Assembly.GetAssembly(typeof(GuidMacro));
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes<MacroAttribute>().Any()
                                     && t.IsAssignableTo(typeof(MacroQueryResult)))
                         .ToList();

        serviceCollection.AddTransient(sp => new Lazy<ISearchService>(sp.GetRequiredService<ISearchService>));
        foreach (var type in found)
        {
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(MacroQueryResult), type));
        }

        return serviceCollection;
    }

    #endregion
}