using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels.Controls;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public sealed class KeywordsViewModelTest : ViewModelTester<KeywordsViewModel>
{
    #region Constructors

    public KeywordsViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors? visitors
    )
    {
        serviceCollection.AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<ISettingsProvider<ApplicationSettings>>()
                         .AddSingleton<IAliasManagementService, AliasManagementService>()
                         .AddSingleton<IAliasValidationService, AliasValidationService>()
                         .AddMockSingleton<IUserGlobalNotificationService>()
                         .AddMockSingleton<IPackagedAppSearchService>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddMockSingleton<IViewFactory>((sp, i) => visitors?.VisitViewFactory?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserDialogueService>((sp, i)
                             => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserNotificationService>((sp, i)
                             => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IProcessLauncher>((sp, i)
                             => visitors?.VisitProcessLauncher?.Invoke(sp, i) ?? i
                         )
                         .AddSingleton<IUserCommunicationService, UserCommunicationService>()
                         .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    public static IEnumerable<object[]> FeedAdditionalParametersWithEmptyTrailingLine()
    {
        yield return
        [
            """
            para1, undeuxtrois
            para2, quatrecinq

            """
        ];
        yield return
        [
            """
            para1,undeuxtrois
            para2,quatrecinq
            """
        ];
        yield return
        [
            """
            para1 , undeuxtrois
            para2 , quatrecinq
            """
        ];
    }

    [Fact]
    public async Task Whe_soft_deleting_alias_Then_it_is_not_anymore_in_search_results()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) => {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";
                const string fileName = "SomeFileName";

                // ACT
                await viewModel.CreateNewAlias(name, fileName);
                await viewModel.DeleteCurrentAliasCommand.ExecuteAsync(null);

                // ASSERT
                const string sql = """
                                   select 
                                       a.Id          as Id,
                                   	    an.name      as Name,
                                   	    case
                                   		    when a.deleted_at is null then false
                                   		    else true
                                   	    end as FieldValue
                                   from alias a 
                                   inner join alias_name an on a.id = an.id_alias 
                                   """;
                var result = db.WithConnection(c => c.Query<DynamicAlias<bool>>(sql, new { name = (string[])[name] }))
                               .ToArray();

                result.ShouldSatisfyAllConditions(
                    r => r.Length.ShouldBeGreaterThan(0),
                    r => r[0].Id.ShouldBeGreaterThan(0),
                    r => r[0].Name.ShouldBe(name),
                    r => r[0].FieldValue.ShouldBeTrue("the field 'deleted_at' has to indicate deletion")
                );
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public async Task When_crating_new_alias_with_additional_parameters_Then_additional_parameters_are_in_db()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                var parameter = new AdditionalParameter { Name = "SomeTestName", Parameter = "SomeParameter" };
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns(_ => (IsConfirmed: true, DataContext: parameter));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";

                // ACT
                await viewModel.CreateNewAlias(name);
                await viewModel.AddParameterCommand.ExecuteAsync(null);
                await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

                // ASSERT
                const string sql = "select count(*) from alias_argument";
                var count = db.WithConnection(c => c.ExecuteScalar(sql));
                count.ShouldBe(1);
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public async Task When_create_alias_with_lua_script_Then_script_is_saved_in_db() =>
        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";
                const string script = "some random text that represent a lua script";

                // ACT
                await viewModel.CreateNewAlias(name, luaScript: script);

                //ASSERT
                const string sql = """
                                   select 
                                       a.id         as Id,
                                       an.name      as Name,
                                       a.lua_script as FieldValue
                                   from 
                                       alias a
                                       inner join alias_name an on a.id = an.id_alias
                                   """;
                var res = db.WithConnection(c => c.Query<DynamicAlias<string>>(sql))
                            .ToArray();

                res.ShouldSatisfyAllConditions(
                    r => r.Length.ShouldBe(1),
                    r => r[0].FieldValue.ShouldBe(script)
                );
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_create_an_alias_with_add_keyword_Then_newly_created_alias_is_in_search_results()
    {
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) => {
                // ARRANGE
                const string cmdName = "add";
                const string parameters = "aliasToCreate";
                var cmdline = new Cmdline(cmdName, parameters);

                // ACT
                await viewModel.LoadAliasesCommand.ExecuteAsync(null); // Simulate navigate to this page once

                viewModel.CreateAliasCommand.Execute(new AddAliasMessage(cmdline));
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);

                // ASSERT
                viewModel.ShouldSatisfyAllConditions(
                    vm => vm.SelectedAlias.ShouldNotBeNull(),
                    vm => vm.Aliases.Count.ShouldBe(4),
                    vm => vm.SelectedAlias!.Id.ShouldBe(0),
                    vm => vm.SelectedAlias!.Name.ShouldBe(parameters),
                    vm => vm.SelectedAlias!.Synonyms.ShouldBe(parameters)
                );
            },
            sqlBuilder
        );
    }

    [Fact]
    public async Task When_creating_alias_with_name_of_soft_deleted_alias_Then_creation_is_impossible()
    {
        IUserNotificationService userNotificationService = null!;
        var visitors = new ServiceVisitors
        {
            VisitUserNotificationService = (_, i) => {
                userNotificationService = i;
                return i;
            },
            VisitUserInteractionService = (_, i) => {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";
                const string fileName = "SomeFileName";

                // ACT
                await viewModel.CreateNewAlias(name, fileName); // Create alias
                await viewModel.DeleteCurrentAliasCommand.ExecuteAsync(null); // Delete alias
                await viewModel.CreateNewAlias(name, fileName); // Recreate the alias

                // ASSERT
                const string sql = """
                                   select 
                                       a.Id as Id,
                                   	    an.name as Name,
                                   	    case
                                   		    when a.deleted_at is null then false
                                   		    else true
                                   	    end as FieldValue
                                   from alias a 
                                   inner join alias_name an on a.id = an.id_alias;
                                   """;
                var result = db.WithConnection(c => c.Query<DynamicAlias<bool>>(sql));
                result = result.ToArray();

                Assert.Multiple(
                    () => result.ShouldSatisfyAllConditions(
                        r => r.Count().ShouldBe(1),
                        r => r.First().Id.ShouldBeGreaterThan(0),
                        r => r.First().Name.ShouldBe(name),
                        r => r.First().FieldValue.ShouldBeTrue("because the alias has been deleted logically")
                    ),
                    // This is the warning saying the alias name is already used for a deleted alias
                    () => userNotificationService.Received().Warning(Arg.Any<string>(), Arg.Any<string>())
                );
            },
            Sql.Empty,
            visitors
        );
    }

    [Theory]
    [MemberData(nameof(FeedAdditionalParametersWithEmptyTrailingLine))]
    public async Task When_creating_multiple_parameters_with_empty_trailing_line_Then_no_error_raised(
        string additionalParameters)
    {
        var sqlBuilder = Sql.Empty;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) => {
                var parameter = new MultipleAdditionalParameterViewModel { RawParameters = additionalParameters };
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns(_ => (IsConfirmed: true, DataContext: parameter));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";

                // ACT
                await viewModel.CreateNewAlias(name);
                await viewModel.AddMultiParametersCommand.ExecuteAsync(null);
                await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

                // ASSERT
                const string sql = "select count(*) from alias_argument";
                var count = db.WithConnection(c => c.ExecuteScalar(sql));
                count.ShouldBe(2);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task When_loading_all_aliases_without_new_creation_Then_always_same_result_returned()
    {
        var builder = new SqlBuilder().AppendAlias(a => a.WithFileName("un")
                                                         .WithArguments("params un")
                                                         .WithSynonyms("deux")
                                      )
                                      .AppendAlias(a => a.WithFileName("deux")
                                                         .WithArguments("params deux")
                                                         .WithSynonyms("trois")
                                      );
        var visitor = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        await TestViewModelAsync(
            async (viewModel, _) => {
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(2);

                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                viewModel.Aliases.Count.ShouldBe(2);
            },
            builder,
            visitor
        );
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task When_search_with_empty_criterion_Then_all_results_shown(string criterion)
    {
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) => {
                // ARRANGE

                // ACT
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                viewModel.SearchCommand.Execute(criterion);

                // ASSERT
                viewModel.Aliases.Count.ShouldBe(3);
            },
            sqlBuilder
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task When_soft_deleting_alias_Then_cache_is_refreshed(int countToAdd)
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) => {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, _) => {
                // ARRANGE
                const string name = "SomeTestName";
                const string fileName = "SomeFileName";

                // ACT
                for (var i = 0; i < countToAdd; i++) await viewModel.CreateNewAlias(name, $"{fileName}_{i}");
                await viewModel.DeleteCurrentAliasCommand.ExecuteAsync(null);
                viewModel.SearchCommand.Execute(string.Empty);

                // ASSERT
                viewModel.Aliases.Count.ShouldBe(countToAdd - 1, "because the alias has been deleted logically");
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public async Task When_tying_to_save_without_filename_or_no_uwp_app_selected_Then_no_save_executed()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) => {
                // Configured to say yes when it'll be asked to delete the alias
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
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";

                // ACT
                viewModel.PrepareAliasForCreation(name);
                await viewModel.SetPackagedApplicationCommand.ExecuteAsync(null);
                viewModel.SearchCommand.Execute(string.Empty);

                // ASSERT
                const string sql = "select count(*) from alias";
                db.WithConnection(c => c.ExecuteScalar<int>(sql))
                  .ShouldBe(0);
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public async Task When_updating_lua_script_Then_db_is_updated() =>
        await TestViewModelAsync(
            async (viewModel, db) => {
                // ARRANGE
                const string name = "SomeTestName";
                const string script = "some random text that represent a lua script";
                const string script2 = "7a3f4c8f-9142-44f0-8455-6818e11845c3";

                // ACT
                await viewModel.CreateNewAlias(name, luaScript: script);

                viewModel.SelectedAlias.ShouldNotBeNull("it is already selected for update");
                viewModel.SelectedAlias!.LuaScript = script2;
                await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

                // ASSERT
                const string sql = """
                                   select 
                                       a.id         as Id,
                                       an.name      as Name,
                                       a.lua_script as FieldValue
                                   from 
                                       alias a
                                       inner join alias_name an on a.id = an.id_alias
                                   """;
                var res = db.WithConnection(c => c.Query<DynamicAlias<string>>(sql))
                            .ToArray();

                res.Length.ShouldBe(1);
                var alias = res[0];
                alias.FieldValue.ShouldBe(script2);
            },
            Sql.Empty
        );

    [Fact]
    public async Task When_using_add_keyword_to_create_alias_Then_the_alias_is_created()
    {
        var sqlBuilder = new SqlBuilder();
        sqlBuilder.AppendAlias(a => a.WithSynonyms())
                  .AppendAlias(a => a.WithSynonyms())
                  .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) => {
                // ARRANGE
                const string name = "add";
                const string parameters = "aliasToCreate";
                var cmdline = new Cmdline(name, parameters);

                // ACT
                await viewModel.LoadAliasesCommand.ExecuteAsync(null); // Simulate first navigation to this page

                WeakReferenceMessenger.Default.Send(new AddAliasMessage(cmdline));
                await viewModel.LoadAliasesCommand.ExecuteAsync(null); // Simulate navigate to this page
                await viewModel.SaveCurrentAliasCommand.ExecuteAsync(cmdline);

                // ASSERT
                viewModel.ShouldSatisfyAllConditions(
                    vm => vm.SelectedAlias.ShouldNotBeNull(),
                    vm => vm.Aliases.Count.ShouldBe(4),
                    vm => vm.SelectedAlias!.Name.ShouldBe(parameters)
                );
            },
            sqlBuilder
        );
    }

    #endregion
}