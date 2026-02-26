using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Win32.Helpers;

/// <summary>
///     Provides extension methods for registering <see cref="IStaThreadRunner" /> in the DI container.
/// </summary>
public static class StaThreadRunnerExtensions
{
    #region Methods

    /// <summary>
    ///     Enables COM interop for Windows Shell APIs, which require execution on a dedicated STA thread.
    ///     This registration ensures that any component needing STA-compatible execution can safely
    ///     offload work without blocking the calling thread or violating COM threading constraints.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register into.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddStaThreadRunner(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStaThreadRunner, StaThreadRunner>();
        return serviceCollection;
    }

    #endregion
}