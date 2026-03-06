using Dapper;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Generators;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class DataReconciliationViewModelTest : ViewModelTester<DataReconciliationViewModel>
{
    #region Constructors

    public DataReconciliationViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static IEnumerable<object[]> DoubloonCriteriaSource()
    {
        const string fileName = "SomeFileName";
        const string arguments = "SomeArguments";

        yield return
        [
            "Notes differ — should be doubloons",
            2,
            new SqlBuilder()
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithNotes("note version A")
                     .WithSynonyms("a1"))
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithNotes("note version B")
                     .WithSynonyms("a2"))
        ];
        yield return
        [
            "Icon differs — should be doubloons",
            2,
            new SqlBuilder()
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithIcon("icon-a.png")
                     .WithSynonyms("a1"))
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithIcon("icon-b.png")
                     .WithSynonyms("a2"))
        ];
        yield return
        [
            "run_as differs — should NOT be doubloons",
            0,
            new SqlBuilder()
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithRunAs(Constants.RunAs.Admin)
                     .WithSynonyms("a1"))
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithRunAs(Constants.RunAs.CurrentUser)
                     .WithSynonyms("a2"))
        ];
        yield return
        [
            "lua_script differs — should NOT be doubloons",
            0,
            new SqlBuilder()
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithLuaScript("return version_a")
                     .WithSynonyms("a1"))
                .AppendAlias(a =>
                    a.WithFileName(fileName)
                     .WithArguments(arguments)
                     .WithLuaScript("return version_b")
                     .WithSynonyms("a2"))
        ];
    }

    private static IEnumerable<object[]> ShowInactiveAliasesSource()
    {
        yield return [new DateOnly(2020, 11, 01), new DateOnly(2020, 10, 15)];
        yield return [new DateOnly(2020, 11, 15), new DateOnly(2020, 10, 1)];
        yield return [new DateOnly(2020, 10, 31), new DateOnly(2020, 10, 1)];
        yield return [new DateOnly(2020, 10, 10), new DateOnly(2020, 09, 20)];
    }

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors? visitors
    )
    {
        serviceCollection.AddApplicationSettings(stg => visitors?.VisitSettings?.Invoke(stg)
                         )
                         .AddMockSingleton<IViewFactory>()
                         .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                         .AddMockSingleton<IApplicationSettingsProvider>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IReconciliationService, ReconciliationService>()
                         .AddSingleton<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddSingleton<IUserCommunicationService, UserCommunicationService>()
                         .AddMockSingleton<IUserGlobalNotificationService>()
                         .AddMockSingleton<IUserDialogueService>((sp, i)
                             => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserNotificationService>((sp, i)
                             => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                         );

        return serviceCollection;
    }

    [Fact]
    public async Task When_canceling_permanent_deletion_Then_nothing_is_deleted()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(false);
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder()
            .AppendAlias(a => a.WithFileName("name")
                               .WithDeletedAt(DateTime.Now)
                               .WithSynonyms("a1", "a2")
                               .WithAdditionalParameters(
                                   ("1", "un"),
                                   ("2", "deux"),
                                   ("3", "trois")
                               )
                               .WithUsage(
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025"
                               )
            );
        await TestViewModelAsync(
            async (viewModel, db) => {
                // arrange
                await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null);

                // act
                viewModel.Aliases.ElementAt(0).IsSelected = true;
                await viewModel.DeletePermanentlyCommand.ExecuteAsync(null);

                // assert
                db.WithConnection(connection => {
                        connection.ShouldSatisfyAllConditions(
                            c => c.ExecuteScalar<long>("select count(*) from alias_usage where id_alias = 1")
                                  .ShouldBeGreaterThan(0, "usage should be cleared"),
                            c => c.ExecuteScalar<long>("select count(*) from alias_name where id_alias = 1")
                                  .ShouldBeGreaterThan(0, "names should be cleared"),
                            c => c.ExecuteScalar<long>("select count(*) from alias_argument where id_alias = 1")
                                  .ShouldBeGreaterThan(0, "arguments should be cleared"),
                            c => c.ExecuteScalar<long>("select count(*) from alias where id = 1")
                                  .ShouldBeGreaterThan(0, "alias should be cleared")
                        );
                    }
                );
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [InlineData("A", 1)]
    [InlineData("B", 1)]
    [InlineData("C", 1)]
    [InlineData("E", 0)]
    public async Task When_changing_inactivity_threshold_Then_search_does_not_select_inactive_aliases(
        string filter, int count)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>()
                 )
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.InactivityThreshold = 2
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("A")
                                            .WithUsage(DateTime.Now.AddMonths(-10))
                         )
                         .AppendAlias(a => a.WithSynonyms("B")
                                            .WithUsage(DateTime.Now.AddMonths(-10))
                         )
                         .AppendAlias(a => a.WithSynonyms("C")
                                            .WithUsage(DateTime.Now.AddMonths(-10))
                         );
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowInactiveAliasesCommand.ExecuteAsync(null);
                await viewModel.FilterAliasCommand.ExecuteAsync(filter);

                viewModel.Aliases.Count.ShouldBe(count);
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [InlineData("A", 1)]
    [InlineData("B", 1)]
    [InlineData("C", 1)]
    [InlineData("E", 0)]
    public async Task When_changing_rarely_used_threshold_Then_search_does_not_select_inactive_aliases(
        string filter, int count)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>()
                 )
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.LowUsageThreshold = 10
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("A")
                                            .WithUsage(
                                                DateTime.Now.AddMonths(-10),
                                                DateTime.Now.AddMonths(-10)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("B")
                                            .WithUsage(
                                                DateTime.Now.AddMonths(-10),
                                                DateTime.Now.AddMonths(-10)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("C")
                                            .WithUsage(
                                                DateTime.Now.AddMonths(-10),
                                                DateTime.Now.AddMonths(-10)
                                            )
                         );
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.SetLowUsageThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowRarelyUsedAliasesCommand.ExecuteAsync(null);
                await viewModel.FilterAliasCommand.ExecuteAsync(filter);

                viewModel.Aliases.Count.ShouldBe(count);
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [MemberData(nameof(DoubloonCriteriaSource))]
    public async Task When_checking_doubloon_criteria_Then_count_matches(
        string description, int expectedCount, SqlBuilder sqlBuilder)
    {
        OutputHelper.WriteLine($"Test description: {description}");
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };

        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(expectedCount);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_delete_permanently_alias_Then_all_correlated_data_is_deleted()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder()
            .AppendAlias(a => a.WithFileName("name")
                               .WithDeletedAt(DateTime.Now)
                               .WithSynonyms("a1", "a2")
                               .WithAdditionalParameters(
                                   ("1", "un"),
                                   ("2", "deux"),
                                   ("3", "trois")
                               )
                               .WithUsage(
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025",
                                   "01/01/2025"
                               )
            );
        await TestViewModelAsync(
            async (viewModel, db) => {
                // arrange
                await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null);

                // act
                viewModel.Aliases.ElementAt(0).IsSelected = true;
                await viewModel.DeletePermanentlyCommand.ExecuteAsync(null);

                // assert
                db.WithConnection(connection => {
                        connection.ShouldSatisfyAllConditions(
                            c => c.ExecuteScalar("select count(*) from alias_usage where id_alias = 1")
                                  .ShouldBe(0, "usage should be cleared"),
                            c => c.ExecuteScalar("select count(*) from alias_name where id_alias = 1")
                                  .ShouldBe(0, "names should be cleared"),
                            c => c.ExecuteScalar("select count(*) from alias_argument where id_alias = 1")
                                  .ShouldBe(0, "arguments should be cleared"),
                            c => c.ExecuteScalar("select count(*) from alias where id = 1")
                                  .ShouldBe(0, "alias should be cleared")
                        );
                    }
                );
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_executing_ShowAliasesWithoutNotesCommand_then_no_error() =>
        await TestViewModelAsync(
            async (viewModel, _) => {
                var t = await Record.ExceptionAsync(async ()
                    => await viewModel.ShowAliasesWithoutNotesCommand.ExecuteAsync(null)
                );
                t.ShouldBeNull();
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_executing_ShowBrokenAliasesCommand_then_no_error() =>
        await TestViewModelAsync(
            async (viewModel, _) => {
                var t = await Record.ExceptionAsync(async ()
                    => await viewModel.ShowBrokenAliasesCommand.ExecuteAsync(null)
                );
                t.ShouldBeNull();
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_executing_ShowDoubloonsCommand_then_no_error() =>
        await TestViewModelAsync(
            async (viewModel, _) => {
                var t = await Record.ExceptionAsync(async ()
                    => await viewModel.ShowDoubloonsCommand.ExecuteAsync(null)
                );
                t.ShouldBeNull();
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_filer_never_used_aliases_Then_filter_works_as_expected()
    {
        var sqlBuilder = new SqlBuilder().AppendAlias(a => a.WithSynonyms("A"))
                                         .AppendAlias(a => a.WithSynonyms("B"))
                                         .AppendAlias(a => a.WithSynonyms("C"));
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowUnusedAliasesCommand.ExecuteAsync(null);

                viewModel.Aliases.Count.ShouldBe(3);
            },
            sqlBuilder
        );
    }

    [Fact]
    public async Task When_filer_rarely_used_aliases_Then_filter_works_as_expected()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>()
                 )
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.LowUsageThreshold = 3
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("A")
                                            .WithUsage(
                                                DateTime.Now.AddMonths(-1), // Recent usage
                                                DateTime.Now.AddMonths(-100)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("B")
                                            .WithUsage(
                                                DateTime.Now.AddMonths(-100),
                                                DateTime.Now.AddMonths(-110),
                                                DateTime.Now.AddMonths(-120)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("C")
                                            .WithUsage(
                                                DateTime.Now,
                                                DateTime.Now.AddMonths(-1), // Recent usage
                                                DateTime.Now.AddMonths(-120),
                                                DateTime.Now.AddMonths(-121),
                                                DateTime.Now.AddMonths(-122),
                                                DateTime.Now.AddMonths(-123),
                                                DateTime.Now.AddMonths(-124)
                                            )
                         );
        await TestViewModelAsync(
            async (viewModel, _) => {
                var t = await Record.ExceptionAsync(async () => {
                    await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                    await viewModel.ShowRarelyUsedAliasesCommand.ExecuteAsync(null);
                });
                t.ShouldBeNull();

                viewModel.Aliases.Count.ShouldBe(1);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_filer_restored_aliases_Then_filter_works_as_expected() =>
        await TestViewModelAsync(
            async (viewModel, _) => {
                var t = await Record.ExceptionAsync(async ()
                    => await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null)
                );
                t.ShouldBeNull();
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_merged_alias_Then_no_soft_deleted_aliases_correlated_to_the_merge_is_found()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var fileName = Generate.Text();
        var arguments = Generate.Text();
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithDeletedAt(DateTime.Now)
                                            .WithSynonyms("a1", "a2")
                         )
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("b1", "b2")
                         )
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("c1", "c2")
                         );
        await TestViewModelAsync(
            async (viewModel, db) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases)
                {
                    item.IsSelected = true;
                }

                await viewModel.MergeCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(0);

                const string sql = "select count(*) from alias where deleted_at is not null";
                db.WithConnection(c => c.ExecuteScalar(sql)).ShouldBe(0, "merging should undelete alias");
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_merged_aliases_Then_all_the_correlated_tables_are_updated()
    {
        var fileName = Generate.Text();
        var arguments = Generate.Text();
        var now = DateTime.Now;
        var timeOffset = 0;

        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("a1", "a2", "a3")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                                            .WithUsage(
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset)
                                            )
                         )
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("a4", "a5", "a6")
                                            .WithAdditionalParameters(
                                                ("params3", "params three"),
                                                ("params4", "params four")
                                            )
                                            .WithUsage(
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset),
                                                now.AddHours(++timeOffset)
                                            )
                         );
        await TestViewModelAsync(
            async (viewModel, db) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases)
                {
                    item.IsSelected = true;
                }

                await viewModel.MergeCommand.ExecuteAsync(null);

                // assert
                viewModel.Aliases.Count.ShouldBe(0);
                const string sql = "select name from alias_argument where id_alias = @IdAlias";
                const string sql2 = "select name from alias_name where id_alias = @IdAlias";
                const string sql3 = "select id from alias_usage where id_alias = @IdAlias";
                db.ShouldSatisfyAllConditions(
                    d => d.WithConnection(c => c.Query<string>(sql, new { IdAlias = 1 })).Count().ShouldBe(4),
                    d => d.WithConnection(c => c.Query<string>(sql2, new { IdAlias = 1 })).Count().ShouldBe(6),
                    d => d.WithConnection(c => c.Query<string>(sql3, new { IdAlias = 1 })).Count().ShouldBe(8)
                );
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [ClassData(typeof(DoubloonGenerator))]
    public async Task When_merged_aliases_Then_only_one_is_found(ISqlBuilder sqlBuilder)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases)
                {
                    item.IsSelected = true;
                }

                await viewModel.MergeCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(0);
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [ClassData(typeof(DoubloonWithNullGenerator))]
    public async Task When_properties_are_null_Then_it_does_not_affect_doubloon_retrieving(
        string description, int doubloons, SqlBuilder sqlBuilder)
    {
        OutputHelper.WriteLine($"Test description: {description}");
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(doubloons);
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [MemberData(nameof(ShowInactiveAliasesSource))]
    public async Task When_retriving_aliases_with_recent_usage_Then_filter_works_as_expected(
        DateOnly lastUsage, DateOnly today)
    {
        var todayDate = today.ToDateTime(TimeOnly.MinValue);
        var lastUsageDate = lastUsage.ToDateTime(TimeOnly.MinValue);

        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>()
                 )
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.InactivityThreshold = 1
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("A")
                                            .WithUsage(
                                                lastUsageDate.AddMonths(-1), // Recent usage
                                                lastUsageDate.AddMonths(-100)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("B")
                                            .WithUsage(
                                                lastUsageDate.AddMonths(-100),
                                                lastUsageDate.AddMonths(-110),
                                                lastUsageDate.AddMonths(-120)
                                            )
                         )
                         .AppendAlias(a => a.WithSynonyms("C")
                                            .WithUsage(
                                                lastUsageDate,
                                                lastUsageDate.AddMonths(-1), // Recent usage
                                                lastUsageDate.AddMonths(-120),
                                                lastUsageDate.AddMonths(-121),
                                                lastUsageDate.AddMonths(-122),
                                                lastUsageDate.AddMonths(-123),
                                                lastUsageDate.AddMonths(-124)
                                            )
                         );
        await TestViewModelAsync(
            async (viewModel, _) => {
                viewModel.OverrideToday(todayDate);
                await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowInactiveAliasesCommand.ExecuteAsync(null);

                //Only alias B has recent usage...
                viewModel.Aliases.Count.ShouldBe(1);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_searching_doubloons_then_doubloons_are_showed()
    {
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        var fileName = Generate.Text();
        var arguments = Generate.Text();

        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("a1", "a2", "a3")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                         )
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("a4", "a5", "a6")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                         )
                         .AppendAlias(a => a.WithFileName(fileName)
                                            .WithArguments(arguments)
                                            .WithSynonyms("a4", "a5", "a6")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                         );

        OutputHelper.WriteLine(
            $"""
             SQL script:
             {sqlBuilder.ToSql()}
             """
        );

        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(3);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_searching_doubloons_Then_they_can_be_found()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithFileName("FileName")
                                            .WithArguments("null")
                                            .WithSynonyms("a1", "a2", "a3")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                         )
                         .AppendAlias(a => a.WithFileName("FileName")
                                            .WithArguments("null")
                                            .WithSynonyms("a4", "a5", "a6")
                                            .WithAdditionalParameters(
                                                ("params1", "params one"),
                                                ("params2", "params two")
                                            )
                         );
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(2);
            },
            sqlBuilder,
            visitors
        );
    }

    #endregion
}