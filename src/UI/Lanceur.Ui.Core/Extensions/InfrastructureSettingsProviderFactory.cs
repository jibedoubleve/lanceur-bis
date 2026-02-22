using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Repositories;
using Lanceur.SharedKernel.IoC;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.Extensions;

/// <summary>
/// Centralises the creation and registration of <see cref="IInfrastructureSettingsProvider"/>.
/// <para>
/// The logger configuration (Serilog) requires access to <see cref="IInfrastructureSettingsProvider"/>
/// during the host bootstrap phase, before the DI container is fully built. Using
/// <c>BuildServiceProvider()</c> at this stage is discouraged as it causes singleton
/// duplication and potential resource leaks.
/// </para>
/// <para>
/// To ensure consistency between the DI-registered instance and the instance used during logger
/// setup, both are created through the same factory logic (<see cref="GetInstance"/>). This
/// guarantees that the logger is always configured with the same provider implementation as the
/// rest of the application, regardless of the build configuration (DEBUG vs Release).
/// </para>
/// </summary>
public static class InfrastructureSettingsProviderFactory
{
    /// <summary>
    /// Registers <see cref="IInfrastructureSettingsProvider"/> as a singleton in the DI container
    /// using the same conditional logic as <see cref="GetInstance"/>.
    /// </summary>
    public static IServiceCollection RegisterInfrastructureSettingsProvider(this IServiceCollection services)
    {
        services
            .AddSingletonConditional<
                IInfrastructureSettingsProvider,
                MemoryInfrastructureSettingsProvider,
                JsonInfrastructureSettingsProvider>();
        return services;
    }

    /// <summary>
    /// Creates and returns an <see cref="IInfrastructureSettingsProvider"/> instance outside of the
    /// DI container. Returns <see cref="MemoryInfrastructureSettingsProvider"/> in DEBUG builds and
    /// <see cref="JsonInfrastructureSettingsProvider"/> in Release builds, mirroring the registration
    /// logic of <see cref="RegisterInfrastructureSettingsProvider"/>.
    /// </summary>
    public static IInfrastructureSettingsProvider GetInstance()
    {
        var conditional = new Conditional<IInfrastructureSettingsProvider>(
            new MemoryInfrastructureSettingsProvider(),
            new JsonInfrastructureSettingsProvider()
        );
        return conditional.Value;
    }
}