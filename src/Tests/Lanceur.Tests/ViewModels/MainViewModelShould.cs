using System.Reflection;
using System.Web.Bookmarks;
using Bogus.Platform;
using Dapper;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Macros;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.ReservedKeywords;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class MainViewModelShould : ViewModelTester<MainViewModel>
{
    #region Constructors

    public MainViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static IEnumerable<object[]> GetKeywords()
    {
        var keywords = typeof(QuitAlias).Assembly.GetTypes()
                                        .Where(t => t.GetCustomAttribute<ReservedAliasAttribute>() is not null)
                                        .Select(t => t.GetCustomAttribute<ReservedAliasAttribute>()!.Name);
        foreach (var type in keywords) yield return new[] { type };
    }

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors visitors
    )
    {
        serviceCollection.AddApplicationSettings(stg => visitors?.VisitSettings?.Invoke(stg))
                         .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                         .AddSingleton(new AssemblySource
                         {
                             MacroSource = Assembly.GetExecutingAssembly(), 
                             ReservedKeywordSource = typeof(MainView).GetAssembly()
                         })
                         .AddMockSingleton<IBookmarkRepositoryFactory>()
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
                         .AddSingleton<IWildcardService, ReplacementComposite>()
                         .AddMockSingleton<ILuaManager>()
                         .AddMockSingleton<IClipboardService>()
                         .AddMockSingleton<IProcessLauncher>((sp, i) 
                             => visitors?.VisitProcessLauncher?.Invoke(sp, i) ?? i
                         )
                         .AddSingleton<IExecutionService, ExecutionService>()
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
        IProcessLauncher sut = null;
        var visitors = new ServiceVisitors
        {
            VisitProcessLauncher = (_, i) =>
            {
                sut = i;
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // ARRANGE
                var alias = new AliasQueryResult();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.SelectedResult = alias;
                await viewModel.ExecuteCommand.ExecuteAsync(false); //RunAsAdmin: false

                // ASSERT done in ServiceCollectionConfigurator
                Assert.Multiple(
                    () => sut.ShouldNotBeNull(),
                    () => sut.Received().Start(Arg.Any<ProcessContext>())
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
    [MemberData(nameof(GetKeywords))]
    public async Task ShouldIncrementUsageOfKeyword(string keyword)
    {
        await TestViewModelAsync(async (viewModel, db) =>
            {
                // Arrange
                
                // Act
                viewModel.Query = keyword;
                await viewModel.SearchCommand.ExecuteAsync(null);
                
                viewModel.SelectedResult = viewModel.Results.FirstOrDefault();
                await viewModel.ExecuteCommand.ExecuteAsync(false);
                
                // Assert
                viewModel.SelectedResult.ShouldNotBeNull();
                viewModel.SelectedResult.Name.ShouldNotBeNull();
                
                const string sql = "select count(*) from alias_usage";
                db.WithConnection(c => c.ExecuteScalar(sql).ShouldBe(1, customMessage: $"We should find the usage of the keyword {keyword}"));
            }
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
        IConfigurationFacade configuration = null;
        var visitors = new ServiceVisitors { VisitSettings = s => configuration = s };
        TestViewModel(
            (viewModel, _) =>
            {
                Assert.All(
                    callsOfShowLastQuery.Select((expected, i) => (expected, i)),
                    t =>
                    {
                        configuration.Application.SearchBox.ShowLastQuery = t.expected;
                        viewModel.ShowLastQuery.ShouldBe(t.expected, $"this is the call n° {t.i + 1} of the test");
                    });

            },
            Sql.Empty,
            visitors
        );
    }

    #endregion
}