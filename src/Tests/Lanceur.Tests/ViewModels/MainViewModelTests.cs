using System.Reflection;
using System.Web.Bookmarks;
using Bogus.Platform;
using Dapper;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Helpers;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public sealed class MainViewModelTests : ViewModelTester<MainViewModel>
{
    #region Constructors

    public MainViewModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static IEnumerable<object[]> GetBuiltinKeywords()
    {
        var keywords = typeof(QuitAlias).Assembly.GetTypes()
                                        .Where(t => t.GetCustomAttribute<ReservedAliasAttribute>() is not null)
                                        .Select(t => t.GetCustomAttribute<ReservedAliasAttribute>()!.Name);
        foreach (var type in keywords)
        {
            yield return [type];
        }
    }

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors? visitors
    )
    {
        serviceCollection.AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                         .AddSingleton(
                             new AssemblySource
                             {
                                 MacroSource = Assembly.GetExecutingAssembly(),
                                 ReservedKeywordSource = typeof(MainView).GetAssembly()
                             }
                         )
                         .AddMockSingleton<IBookmarkRepositoryFactory>()
                         .AddSingleton<ISearchService, SearchService>()
                         .AddSingleton<IMacroAliasExpanderService, MacroAliasExpanderService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<ISettingsProvider<ApplicationSettings>>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddMockSingleton<IUserDialogueService>()
                         .AddMockSingleton<IUserGlobalNotificationService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IUserCommunicationService>()
                         .AddSingleton<IMemoryCache, MemoryCache>()
                         .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                         .AddSingleton<IWildcardService, ReplacementComposite>()
                         .AddMockSingleton<ILuaManager>()
                         .AddMockSingleton<IClipboardService>()
                         .AddMockSingleton<IProcessLauncher>((sp, i)
                             => visitors?.VisitProcessLauncher?.Invoke(sp, i) ?? i
                         )
                         .AddSingleton<IWatchdogBuilder>(_ => visitors?.WatchdogBuilder ?? new TestWatchdogBuilder())
                         .AddSingleton<IExecutionService, ExecutionService>();

        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, AliasStore>());
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, ReservedAliasStore>());
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, CalculatorStore>());

        return serviceCollection;
    }

    [Theory]
    [InlineData(new[] { true, false })]
    [InlineData(new[] { false, true })]
    public void When_ShowLastQuery_setting_changes_Then_view_model_reflects_it(bool[] callsOfShowLastQuery)
    {
        MemoryApplicationSettingsProvider config = null!;

        var visitors = new ServiceVisitors { VisitApplicationSettingsProvider = c => config = c };
        TestViewModel(
            (viewModel, _) => {
                Assert.All(
                    callsOfShowLastQuery.Select((expected, i) => (expected, i)),
                    t => {
                        config.Value.SearchBox.ShowLastQuery = t.expected;
                        viewModel.ShowLastQuery.ShouldBe(t.expected, $"this is the call n° {t.i + 1} of the test");
                    }
                );
            },
            Sql.Empty,
            visitors
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_ShowResult_setting_varies_Then_results_are_displayed_accordingly(bool showAllResults)
    {
        var sqlBuilder = new SqlBuilder().AppendAlias(a => a.WithSynonyms("alias1", "alias_1"))
                                         .AppendAlias(a => a.WithSynonyms("alias2", "alias_2"))
                                         .AppendAlias(a => a.WithSynonyms("alias3", "alias_3"));

        var visitors = new ServiceVisitors
        {
            VisitApplicationSettingsProvider = s => s.Value.SearchBox.ShowResult = showAllResults
        };
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.DisplayResultsIfAllowed();
                /* If showAllResults is true, then more than one result is expected user displays the search box results
                 * Otherwise, when showAllResults is false, then zero result are displayed in the search box results
                 */
                showAllResults.ShouldBe(viewModel.Results.Count > 0);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_executing_alias_Then_process_is_started()
    {
        IProcessLauncher sut = null!;
        var visitors = new ServiceVisitors
        {
            VisitProcessLauncher = (_, i) => {
                sut = i;
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) => {
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

    /// <summary>
    ///     Regression test for bug #1185: executing a builtin keyword multiple times
    ///     should reuse the existing dummy alias, not create a duplicate.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetBuiltinKeywords))]
    public async Task When_executing_builtin_keyword_repeatedly_Then_usage_is_tracked_on_one_alias(string keyword) =>
        await TestViewModelAsync(
            async (viewModel, db) => {
                const int count = 5;
                for (var i = 0; i < count; i++)
                {
                    viewModel.Query = keyword;
                    await viewModel.SearchCommand.ExecuteAsync(null);
                    await viewModel.ExecuteCommand.ExecuteAsync(false);
                }

                // Assert
                const string sqlDistinctAliases = "select count(distinct id_alias) from alias_usage";
                const string sqlCountUsage = "select count(*) from alias_usage";

                Assert.Multiple(
                    () => db.WithConnection(c => c.ExecuteScalar(sqlDistinctAliases).ShouldBe(1)),
                    () => db.WithConnection(c => c.ExecuteScalar(sqlCountUsage).ShouldBe(count))
                );
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_executing_regular_alias_repeatedly_Then_usage_is_tracked_on_one_alias() =>
        await TestViewModelAsync(
            async (viewModel, db) => {
                // Arrange
                const int count = 5;
                var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms("alias1", "alias_1"))
                                          .ToSql();
                db.WithConnection(c => c.Execute(sql));

                // Act
                for (var i = 0; i < count; i++)
                {
                    viewModel.Query = "alias1";
                    await viewModel.SearchCommand.ExecuteAsync(null);
                    await viewModel.ExecuteCommand.ExecuteAsync(false);
                }

                // Assert
                const string sqlDistinctAliases = "select count(distinct id_alias) from alias_usage";
                const string sqlCountUsage = "select count(*) from alias_usage";

                Assert.Multiple(
                    () => db.WithConnection(c => c.ExecuteScalar(sqlDistinctAliases).ShouldBe(1)),
                    () => db.WithConnection(c => c.ExecuteScalar(sqlCountUsage).ShouldBe(count))
                );
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_executing_while_search_is_pending_Then_alias_is_executed()
    {
        IProcessLauncher processLauncher = null!;
        var visitors = new ServiceVisitors
        {
            WatchdogBuilder = new PendingWatchdogBuilder(),
            VisitProcessLauncher = (_, i) => {
                processLauncher = i;
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(a => a.WithSynonyms("alias1"));

        await TestViewModelAsync(
            async (viewModel, _) => {
                // Arrange
                viewModel.Query = "alias1";
                await viewModel.SearchCommand.ExecuteAsync(null);
                viewModel.SelectedResult.ShouldBeNull();

                // Act
                await viewModel.ExecuteCommand.ExecuteAsync(false);

                // Assert
                processLauncher.Received(1).Start(Arg.Any<ProcessContext>());
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_searching_aliases_Then_all_names_are_returned()
    {
        // ARRANGE
        var sqlBuilder = new SqlBuilder().AppendAlias(a => a.WithSynonyms("alias1", "alias_1"))
                                         .AppendAlias(a => a.WithSynonyms("alias2", "alias_2"))
                                         .AppendAlias(a => a.WithSynonyms("alias3", "alias_3"));


        await TestViewModelAsync(
            async (viewModel, _) => {
                // ACT
                viewModel.Query = "alias";
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT
                viewModel.Results.Count.ShouldBe(6);
            },
            sqlBuilder
        );
    }

    [Theory]
    [InlineData("2+5", "7")]
    [InlineData("2 + 5", "7")]
    [InlineData("2 * 5", "10")]
    public async Task When_searching_calculation_Then_result_is_first_item(string operation, string result) =>
        await TestViewModelAsync(async (viewModel, _) => {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.Query = operation;
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT done in ServiceCollectionConfigurator
                viewModel.ShouldSatisfyAllConditions(
                    vm => vm.Results.Count.ShouldBeGreaterThan(0),
                    vm => vm.Results[0].Name.ShouldBe(result)
                );
            }
        );

    [Fact]
    public async Task When_searching_with_ShowResult_enabled_Then_only_matching_results_are_shown()
    {
        var sqlBuilder = new SqlBuilder().AppendAlias(a => a.WithSynonyms())
                                         .AppendAlias(a => a.WithSynonyms())
                                         .AppendAlias(a => a.WithSynonyms())
                                         .AppendAlias(a => a.WithSynonyms());
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitApplicationSettingsProvider = s => {
                s.Value.SearchBox.ShowLastQuery = true;
                s.Value.SearchBox.ShowResult = true;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) => {
                const int expectedCount = 1;
                viewModel.Query = "alias_1"; // default name is alias_{idAlias}

                // Handle the option "Application.SearchBox.ShowResult" If sets to "True" it means it should show
                // all the results (only if query is empty)
                await viewModel.DisplayResultsIfAllowed();
                await viewModel.SearchCommand.ExecuteAsync(null);

                viewModel.Results.Count.ShouldBe(expectedCount);
            },
            sqlBuilder,
            visitors
        );
    }

    #endregion
}