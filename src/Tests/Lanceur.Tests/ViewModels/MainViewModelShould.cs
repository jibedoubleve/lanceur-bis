using System.Reflection;
using Shouldly;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Managers;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class MainViewModelShould : ViewModelTester<MainViewModel>
{
    #region Constructors

    public MainViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors visitors
    )
    {
        serviceCollection.AddApplicationSettings(stg => visitors?.VisitSettings?.Invoke(stg))
                         .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                         .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                         .AddSingleton<IMappingService, MappingService>()
                         .AddSingleton<ISearchService, SearchService>()
                         .AddSingleton<IMacroService, MacroService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<IDatabaseConfigurationService>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddMockSingleton<IUserInteractionService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IInteractionHubService>()
                         .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                         .AddSingleton<IMemoryCache, MemoryCache>()
                         .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                         .AddMockSingleton<IExecutionService>((sp, i) =>
                             {
                                 i.ExecuteAsync(Arg.Any<ExecutionRequest>())
                                  .Returns(ExecutionResponse.NoResult);
                                 return visitors?.VisitExecutionManager?.Invoke(sp, i) ?? i;
                             }
                         )
                         .AddMockSingleton<ISearchServiceOrchestrator>((_, i) =>
                             {
                                 i.IsAlive(Arg.Any<IStoreService>(), Arg.Any<Cmdline>())
                                  .Returns(true);
                                 return i;
                             }
                         )
                         .AddMockSingleton<IStoreLoader>((sp, i) =>
                             {
                                 i.Load()
                                  .Returns([new AliasStore(sp), new ReservedAliasStore(sp), new CalculatorStore(sp)]);
                                 return i;
                             }
                         );
        return serviceCollection;
    }

    [Fact]
    public async Task BeAbleToExecuteAliases()
    {
        IExecutionService sut = null;
        var visitors = new ServiceVisitors
        {
            VisitExecutionManager = (_, i) =>
            {
                sut = i;
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.SelectedResult = alias;
                await viewModel.ExecuteCommand.ExecuteAsync(false); //RunAsAdmin: false

                // ASSERT done in ServiceCollectionConfigurator
                await Assert.MultipleAsync(
                    () =>
                    {
                        sut.ShouldNotBeNull();
                        return Task.CompletedTask;
                    },
                    () => sut.Received().ExecuteAsync(Arg.Any<ExecutionRequest>())
                );
            },
            Sql.Empty,
            visitors
        );
    }

    [Theory]
    [InlineData("2+5", "7")]
    [InlineData("2 + 5", "7")]
    [InlineData("2 * 5", "10")]
    public async Task BeAbleToExecuteCalculation(string operation, string result)
    {
        await TestViewModelAsync(async (viewModel, _) =>
            {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.Query = operation;
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT done in ServiceCollectionConfigurator
                viewModel.ShouldSatisfyAllConditions(
                    vm => vm.Results.Count.ShouldBeGreaterThan(0),
                    vm => vm.Results.ElementAt(0).Name.ShouldBe(result)
                );
            }
        );
    }

    [Fact]
    public async Task BeAbleToSearchAliases()
    {
        // ARRANGE
        var sqlBuilder = new SqlGenerator().AppendAlias(a => a.WithSynonyms("alias1", "alias_1"))
                                           .AppendAlias(a => a.WithSynonyms("alias2", "alias_2"))
                                           .AppendAlias(a => a.WithSynonyms("alias3", "alias_3"));


        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // ACT
                viewModel.Query = "alias";
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT
                viewModel.Results.Count.ShouldBe(6);
            },
            sqlBuilder
        );
    }

    [Fact]
    public async Task NotShowAllResultWhenPreviousQuery()
    {
        var sqlBuilder = new SqlGenerator().AppendAlias(a => a.WithSynonyms())
                                           .AppendAlias(a => a.WithSynonyms())
                                           .AppendAlias(a => a.WithSynonyms())
                                           .AppendAlias(a => a.WithSynonyms());
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitSettings = s =>
            {
                s.Application.SearchBox.ShowLastQuery = true;
                s.Application.SearchBox.ShowResult = true;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                    const int expectedCount = 1;
                    viewModel.Query = "alias_1"; // default name is alias_{idAlias}
                    
                    // Handle the option "Application.SearchBox.ShowResult" If sets ti "True" it means it should show
                    // all the results (only if query is empty)
                    await viewModel.DisplayResultsIfAllowed();
                    await viewModel.SearchCommand.ExecuteAsync(null);

                    viewModel.ShouldSatisfyAllConditions(
                        vm => vm.Results.Count.ShouldBe(expectedCount),
                        vm => vm.Results.Count.ShouldBe(expectedCount)
                    );
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task ShowAllResultsOrNotDependingOnConfiguration(bool showAllResults, int count)
    {
        var builder = new SqlGenerator().AppendAlias(a => a.WithSynonyms("alias1", "alias_1"))
                                        .AppendAlias(a => a.WithSynonyms("alias2", "alias_2"))
                                        .AppendAlias(a => a.WithSynonyms("alias3", "alias_3"));

        var visitors = new ServiceVisitors
        {
            VisitSettings = settings =>
            {
                var application = new DatabaseConfiguration { SearchBox = { ShowResult = showAllResults } };
                settings.Application.Returns(application);
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.DisplayResultsIfAllowed();
                viewModel.Results.Count.ShouldBeGreaterThanOrEqualTo(count);
            },
            builder,
            visitors
        );
    }

    [Theory]
    [InlineData(new[] { true, false })]
    [InlineData(new[] { false, true })]
    public void ShowLastResultOrNotDependingOnConfiguration(bool[] callsOfShowLastQuery)
    {
        ISettingsFacade settings = null;
        var visitors = new ServiceVisitors { VisitSettings = s => settings = s };
        TestViewModel(
            (viewModel, _) =>
            {
                Assert.All(
                    callsOfShowLastQuery.Select((expected, i) => (expected, i)),
                    t =>
                    {
                        settings.Application.SearchBox.ShowLastQuery = t.expected;
                        viewModel.ShowLastQuery.ShouldBe(t.expected, $"this is the call n° {t.i + 1} of the test");
                    });

            },
            Sql.Empty,
            visitors
        );
    }

    #endregion
}