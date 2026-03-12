using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Ui.WPF.ReservedAliases;

public static class ReservedAliasesRegistrationExtension
{
    #region Methods

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
    public static IServiceCollection AddReservedAliases(
        this IServiceCollection serviceCollection,
        Type assemblyMarker
    )
    {
        var asm = Assembly.GetAssembly(assemblyMarker);
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes<ReservedAliasAttribute>().Any()
                                     && t.IsAssignableTo(typeof(SelfExecutableQueryResult)))
                         .ToList();

        foreach (var type in found)
        {
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(SelfExecutableQueryResult), type));
        }

        return serviceCollection;
    }

    #endregion
}