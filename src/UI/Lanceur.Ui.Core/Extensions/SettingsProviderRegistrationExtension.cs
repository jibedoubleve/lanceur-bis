using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.SharedKernel.IoC;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.Extensions;

/// <summary>
///     Centralises the creation and registration of <see cref="ISettingsProvider{TConfig}" /> for
///     both <see cref="ApplicationSettings" /> and <see cref="InfrastructureSettings" />.
///     <para>
///         The logger configuration (Serilog) requires access to
///         <see cref="ISettingsProvider{TConfig}" /> for <see cref="InfrastructureSettings" />
///         during the host bootstrap phase, before the DI container is fully built. Using
///         <c>BuildServiceProvider()</c> at this stage is discouraged as it causes singleton
///         duplication and potential resource leaks.
///     </para>
///     <para>
///         To ensure consistency between the DI-registered instance and the instance used during
///         logger setup, both are created through the same factory logic (<see cref="GetInfrastructureSettingsProvider" />).
///         This guarantees that the logger is always configured with the same provider implementation
///         as the rest of the application, regardless of the build configuration (DEBUG vs Release).
///     </para>
/// </summary>
public static class SettingsProviderFactory
{
    #region Methods

    /// <summary>
    ///     Creates and returns an <see cref="ISettingsProvider{TConfig}" /> for
    ///     <see cref="InfrastructureSettings" /> outside of the DI container.
    ///     Returns <see cref="MemoryInfrastructureSettingsProvider" /> in DEBUG builds and
    ///     <see cref="JsonInfrastructureSettingsProvider" /> in Release builds, mirroring the
    ///     registration logic of <see cref="AddSettingsProviders" />.
    /// </summary>
    public static ISettingsProvider<InfrastructureSettings> GetInfrastructureSettingsProvider()
    {
        var conditional = new Conditional<ISettingsProvider<InfrastructureSettings>>(
            new MemoryInfrastructureSettingsProvider(),
            new JsonInfrastructureSettingsProvider()
        );
        return conditional.Value;
    }

    /// <summary>
    ///     Registers SettingsProviders as a singleton in the DI container
    ///     using the same conditional logic as <see cref="GetInfrastructureSettingsProvider" />.
    /// </summary>
    public static IServiceCollection AddSettingsProviders(this IServiceCollection services)
    {
        services
            .AddSingleton<ISettingsProvider, SQLiteApplicationSettingsProvider>()
            .AddSingletonConditional<
                ISettingsProvider,
                MemoryInfrastructureSettingsProvider,
                JsonInfrastructureSettingsProvider>();
        return services;
    }

    #endregion
}