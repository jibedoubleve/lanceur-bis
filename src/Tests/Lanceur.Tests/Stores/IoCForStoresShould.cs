using Everything.Wrapper;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Stores;

public class IoCForStoresShould : TestBase
{
    #region Constructors

    public IoCForStoresShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void RegisterAllStores()
    {
        // arrange


        var serviceProvider = new ServiceCollection()
                              .AddTestOutputHelper(OutputHelper)
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

    [Theory]
    [InlineData(null, "pp hello world")] // null override, then use the default one
    [InlineData("^pp.*", ": hello world")]
    public void UseDefaultShortcutWhenNoOverride(string? aliasOverride, string cmdlineString)
    {
        // arrange 
        var cfgOverride = aliasOverride is null
            ? null
            : new ApplicationSettings
            {
                Stores = new StoreSection
                {
                    StoreShortcuts =
                    [
                        new StoreShortcut
                        {
                            AliasOverride = aliasOverride,
                            StoreType = typeof(EverythingStore).ToString()
                        }
                    ]
                }
            };

        var serviceProvider = new ServiceCollection()
                              .AddTestOutputHelper(OutputHelper)
                              .AddSingleton<IStoreService, EverythingStore>()
                              .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                              .AddSingleton<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                              .AddStoreServicesConfiguration(cfgOverride)
                              .AddTestOutputHelper(OutputHelper)
                              .AddStoreServicesMockContext((_, i) => {
                                  i.Value.Returns(new StoreSection
                                  {
                                      StoreShortcuts =
                                      [
                                          new StoreShortcut
                                          {
                                              AliasOverride = aliasOverride,
                                              StoreType = typeof(EverythingStore).FullName
                                          }
                                      ]
                                  });
                                  return i;
                              })
                              .BuildServiceProvider();

        // act
        var store = serviceProvider
                    .GetServices<IStoreService>()
                    .Single(x => x.GetType() == typeof(EverythingStore));
        var orchestrator = serviceProvider.GetService<ISearchServiceOrchestrator>()!;

        // assert
        OutputHelper.WriteLine($"Alive pattern: '{store.StoreOrchestration.AlivePattern}'");
        OutputHelper.WriteLine($"Cmdline      : {cmdlineString}");
        orchestrator.IsAlive(store, Cmdline.Parse(cmdlineString))
                    .ShouldBeFalse();
    }

    [Theory]
    [InlineData(null, ": hello world")] // null override, then use the default one
    [InlineData("^pp.*", "pp hello world")]
    public void UseOverridenShortcutWhenConfigured(string? aliasOverride, string cmdlineString)
    {
        // arrange 
        var cfgOverride = aliasOverride is null
            ? null
            : new ApplicationSettings
            {
                Stores = new StoreSection
                {
                    StoreShortcuts =
                    [
                        new StoreShortcut
                        {
                            AliasOverride = aliasOverride,
                            StoreType = typeof(EverythingStore).ToString()
                        }
                    ]
                }
            };

        var serviceProvider = new ServiceCollection()
                              .AddTestOutputHelper(OutputHelper)
                              .AddSingleton<IStoreService, EverythingStore>()
                              .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                              .AddSingleton<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                              .AddStoreServicesMockContext((_, i) => {
                                  i.Value.Returns(new StoreSection()
                                  {
                                      StoreShortcuts = [new StoreShortcut
                                      {
                                          StoreType = typeof(EverythingStore).FullName,
                                          AliasOverride = aliasOverride
                                      }]
                                  });
                                  return i;
                              })
                              .AddStoreServicesConfiguration(cfgOverride)
                              .BuildServiceProvider();

        // act
        var store = serviceProvider
                    .GetServices<IStoreService>()
                    .Single(x => x.GetType() == typeof(EverythingStore));
        var orchestrator = serviceProvider.GetService<ISearchServiceOrchestrator>()!;

        // assert
        orchestrator.IsAlive(store, Cmdline.Parse(cmdlineString))
                    .ShouldBeTrue();
    }

    [Fact]
    public void UseOverridenShortcutWhenUpdated()
    {
        // arrange
        const string aliasOverride1 = "^pp.*";
        const string aliasOverride2 = "^éé.*";

        const string cmdlineString1 = ": hello world";
        const string cmdlineString2 = "pp hello world";
        const string cmdlineString3 = "éé hello world";

        var connectionManager = GetConnectionManager(SqlBuilder.Empty);
        var serviceProvider = ConfigureServices();

        // act
        var store = serviceProvider
                    .GetServices<IStoreService>()
                    .Single(x => x.GetType() == typeof(EverythingStore));
        var orchestrator = serviceProvider.GetService<ISearchServiceOrchestrator>()!;
        var configuration = serviceProvider.GetService<IConfigurationFacade>()!;

        orchestrator.ShouldSatisfyAllConditions(
            o =>
                // "At this point, there's no configuration, we used the default config (hardcoded)"
                o.IsAlive(store, Cmdline.Parse(cmdlineString1))
                 .ShouldBeTrue("Default values should be used"),
            o => {
                // Let's update the configuration and check whether it is taken into account
                UpdateConfiguration(aliasOverride1);
                o.IsAlive(store, Cmdline.Parse(cmdlineString2))
                 .ShouldBeTrue("When updating from default values to new value");
            },
            o => {
                // Let's do this again to be sure the update can be done multiple times
                UpdateConfiguration(aliasOverride2);
                o.IsAlive(store, Cmdline.Parse(cmdlineString3))
                 .ShouldBeTrue("When updating from some values to updated values");
            });
        return;

        IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection()
                                    .AddTestOutputHelper(OutputHelper)
                                    .AddSingleton<IDbConnectionManager>(connectionManager)
                                    .AddSingleton<IStoreService, EverythingStore>()
                                    .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                                    .AddSingleton<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                                    .AddMockSingleton<IEverythingApi>(); // Real ISection<StoreSection> needed — don't use AddStoreServicesMockContext()

            serviceCollection.AddConfiguration() // Real configuration facility (not mocked)
                             .AddConfigurationSections()
                             .AddSingleton<IInfrastructureSettingsProvider, MemoryInfrastructureSettingsProvider>();
            return serviceCollection.BuildServiceProvider();
        }

        void UpdateConfiguration(string aliasOverride)
        {
            configuration.Application.Stores.StoreShortcuts =
            [
                new StoreShortcut
                {
                    StoreType = typeof(EverythingStore).ToString(),
                    AliasOverride = aliasOverride
                }
            ];
            configuration.Save();
        }
    }

    #endregion
}