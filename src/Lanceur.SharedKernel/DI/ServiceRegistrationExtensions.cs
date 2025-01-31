using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.SharedKernel.DI;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface to facilitate the registration 
/// of types for dependency injection based on their names and assembly metadata.
/// 
/// This class contains methods to register types that end with a specified suffix (e.g., "ViewModel") as either 
/// transient or singleton instances, depending on whether they are decorated with the <see cref="SingletonAttribute"/>.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Registers all types from the assembly of the specified <see cref="Type"/> whose names end with the specified suffix 
    /// (e.g., "ViewModel") into the provided <see cref="IServiceCollection"/> for dependency injection.
    /// 
    /// Types decorated with the <see cref="SingletonAttribute"/> will be registered as singletons, while 
    /// all other types matching the specified suffix will be registered with a transient lifetime.
    /// </summary>
    /// <param name="serviceCollection">
    /// The <see cref="IServiceCollection"/> instance where the types will be registered.
    /// </param>
    /// <param name="suffix">
    /// The suffix used to identify the types to register (e.g., "ViewModel").
    /// </param>
    /// <param name="source">
    /// A <see cref="Type"/> object whose assembly will be scanned for types matching the specified suffix.
    /// </param>
    /// <returns>
    /// Returns the modified <see cref="IServiceCollection"/> to allow for method chaining (fluent interface).
    /// </returns>
    public static IServiceCollection Register(this IServiceCollection serviceCollection, string suffix, Type source) => serviceCollection.Register(suffix, source.Assembly);

    /// <summary>
    /// Registers all types from the specified <see cref="Assembly"/> whose names end with the specified suffix 
    /// (e.g., "ViewModel") into the provided <see cref="IServiceCollection"/> for dependency injection.
    /// 
    /// Types decorated with the <see cref="SingletonAttribute"/> will be registered as singletons, while 
    /// all other types matching the specified suffix will be registered with a transient lifetime.
    /// </summary>
    /// <param name="serviceCollection">
    /// The <see cref="IServiceCollection"/> instance where the types will be registered.
    /// </param>
    /// <param name="suffix">
    /// The suffix used to identify the types to register (e.g., "ViewModel").
    /// </param>
    /// <param name="asm">
    /// The <see cref="Assembly"/> from which types will be scanned for matching names.
    /// </param>
    /// <returns>
    /// Returns the modified <see cref="IServiceCollection"/> to allow for method chaining (fluent interface).
    /// </returns>
    private static IServiceCollection Register(this IServiceCollection serviceCollection, string suffix, Assembly asm)
    {
        var types = asm.GetTypes()
                       .Where(e => e.FullName!.EndsWith(suffix))
                       .ToArray();

        _ = types.Where(e => e.GetCustomAttributes(typeof(SingletonAttribute), true).Length == 0)
                 .Select(serviceCollection.AddTransient)
                 .ToArray();
        
        _ = types.Where(e => e.GetCustomAttributes(typeof(SingletonAttribute), true).Length > 0)
                 .Select(e => serviceCollection.AddSingleton(e))
                 .ToArray();

        return serviceCollection;   
    }
    
    /// <summary>
    /// Registers all types from the assembly identified by the specified assembly name whose names end with the specified suffix 
    /// (e.g., "ViewModel") into the provided <see cref="IServiceCollection"/> for dependency injection.
    /// 
    /// Types decorated with the <see cref="SingletonAttribute"/> will be registered as singletons, while 
    /// all other types matching the specified suffix will be registered with a transient lifetime.
    /// </summary>
    /// <param name="serviceCollection">
    /// The <see cref="IServiceCollection"/> instance where the types will be registered.
    /// </param>
    /// <param name="suffix">
    /// The suffix used to identify the types to register (e.g., "ViewModel").
    /// </param>
    /// <param name="asm">
    /// The name of the assembly (e.g., "MyAssembly") from which types will be scanned for matching names.
    /// </param>
    /// <returns>
    /// Returns the modified <see cref="IServiceCollection"/> to allow for method chaining (fluent interface).
    /// </returns>
    public static IServiceCollection Register(this IServiceCollection serviceCollection, string suffix, string asm) => serviceCollection.Register(suffix, Assembly.Load(asm));
}