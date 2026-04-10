using System.Data;
using Lanceur.Core;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Extensions;
using Lanceur.Infra.Macros;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.ReservedAliases;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.IoC;

public sealed class ServiceRegistrationTest : TestBase
{
    #region Constructors

    public ServiceRegistrationTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    /// <summary>
    ///     Smoke test for the composition root. Verifies that all type-based dependencies
    ///     are satisfied (<see cref="ServiceProviderOptions.ValidateOnBuild" />),
    ///     then explicitly resolves services registered via factory lambda
    ///     that ValidateOnBuild cannot validate automatically.
    /// </summary>
    [Fact]
    public void When_composition_root_is_built_Then_all_services_can_be_resolved()
    {
        // arrange
        var services = new ServiceCollection()
                       .AddMemoryCache()
                       .AddLoggingForTests(OutputHelper)
                       .AddInfraServices()
                       .AddDatabaseServices()
                       .AddWin32Services()
                       .AddUiCoreServices()
                       .AddSettingsInfrastructure()
                       .AddStores()
                       .AddMacros()
                       .AddReservedAliases(typeof(AddAlias))
                       // AddWpfServices() intentionally excluded — requires a WPF dispatcher
                       // Only the services the composition depends on are mocked
                       .AddMockSingleton<IUserGlobalNotificationService>()
                       .AddMockSingleton<IUserDialogueService>()
                       .AddMockSingleton<IUserNotificationService>();

        // act — ValidateOnBuild checks the full type-based dependency graph
        var provider = Should.NotThrow(() =>
            services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true })
        );

        // assert — explicitly resolve services registered via factory lambda, which ValidateOnBuild cannot validate
        provider.ShouldSatisfyAllConditions(
            p => Should.NotThrow(() => p.GetRequiredService<IDbConnection>()),
            p => Should.NotThrow(() => p.GetRequiredService<IDatabaseUpdater>()),
            p => Should.NotThrow(() => p.GetServices<ISettingsProvider>().ShouldNotBeEmpty())
        );
    }

    /// <summary>
    ///     Verifies the automatic store discovery logic via reflection.
    ///     The count is intentional: an added or removed store must be visible here.
    /// </summary>
    [Fact]
    public void When_AddStores_is_called_Then_all_stores_are_loaded_automatically()
    {
        // arrange
        var serviceProvider = new ServiceCollection()
                              .AddLoggingForTests(OutputHelper)
                              .AddStoreServicesConfiguration()
                              .AddStores()
                              .AddStoreServicesMockContext()
                              .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                              .BuildServiceProvider();

        // act
        var stores = serviceProvider.GetServices<IStoreService>();

        // assert
        stores.Count().ShouldBe(7);
    }

    /// <summary>
    ///     Verifies the automatic macro discovery logic via reflection.
    ///     The count is intentional: an added or removed macro must be visible here.
    /// </summary>
    [Fact]
    public void When_AddMacros_is_called_Then_all_macros_are_registered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddMacros();

        // assert
        const int expectedCount = 4;
        services.ShouldSatisfyAllConditions(
            s => s.Count(d => d.ServiceType == typeof(MacroQueryResult))
                  .ShouldBe(expectedCount, $"{expectedCount} macros should be discovered and registered"),
            s => s.Any(d => d.ServiceType == typeof(Lazy<ISearchService>))
                  .ShouldBeTrue("Lazy<ISearchService> should be registered")
        );
    }

    /// <summary>
    ///     Verifies the automatic reserved alias discovery logic via reflection.
    ///     The count is intentional: an added or removed alias must be visible here.
    /// </summary>
    [Fact]
    public void When_AddReservedAliases_is_called_Then_all_reserved_aliases_are_registered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddReservedAliases(typeof(AddAlias));

        // assert
        const int expectedCount = 10;
        services
            .Count(d => d.ServiceType == typeof(SelfExecutableQueryResult))
            .ShouldBe(expectedCount, $"{expectedCount} reserved aliases should be discovered and registered");
    }

    #endregion
}
