using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
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

public class KeywordsViewModelShould : ViewModelTester<KeywordsViewModel>
{
    #region Constructors

    public KeywordsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors visitors
    )
    {
        serviceCollection.AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<IApplicationSettingsProvider>()
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

    [Fact]
    public async Task CreateAliasWithAddKeyword()
    {
        var sqlBuilder = new SqlBuilder();
        sqlBuilder.AppendAlias(a => a.WithSynonyms())
                  .AppendAlias(a => a.WithSynonyms())
                  .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
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

    [Fact]
    public async Task CreateAliasWithLuaScript()
    {
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
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

                res.Length.ShouldBe(1);
                var alias = res[0];
                alias.FieldValue.ShouldBe(script);
            },
            Sql.Empty
        );
    }

    [Fact]
    public async Task CreateAliasWorkOnSecondNavigation()
    {
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // ARRANGE
                const string cmdName = "add";
                const string parameters = "aliasToCreate";
                var cmdline = new Cmdline(cmdName, parameters);

                // ACT
                await viewModel.LoadAliasesCommand.ExecuteAsync(null); // Simulate navigate to this page once

                viewModel.CreateAliasCommand.Execute(new(cmdline));
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
    public async Task DeleteAliasLogically()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) =>
            {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, db) =>
            {
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

    public static IEnumerable<object[]> FeedNotCrashWhenCreatingMultipleParametersWithEmptyTrailingLine()
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
    public async Task HaveViewModelInMessageBoxWhenUpdatingAdditionalParameters()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
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
            async (viewModel, db) =>
            {
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


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task ListAllAliasOnEmptySearch(string criterion)
    {
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms())
                         .AppendAlias(a => a.WithSynonyms());
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
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

    [Fact]
    public async Task NotBeAbleToCreateAliasWithDeletedAliasName()
    {
        IUserNotificationService userNotificationService = null;
        var visitors = new ServiceVisitors
        {
            VisitUserNotificationService = (_, i) =>
            {
                userNotificationService = i;
                return i;
            },
            VisitUserInteractionService = (_, i) =>
            {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                // ARRANGE
                const string name = "SomeTestName";
                const string fileName = "SomeFileName";

                // ACT
                await viewModel.CreateNewAlias(name, fileName);               // Create alias
                await viewModel.DeleteCurrentAliasCommand.ExecuteAsync(null); // Delete alias
                await viewModel.CreateNewAlias(name, fileName);               // Recreate the alias

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
    [MemberData(nameof(FeedNotCrashWhenCreatingMultipleParametersWithEmptyTrailingLine))]
    public async Task NotCrashWhenCreatingMultipleParametersWithEmptyTrailingLine(string additionalParameters)
    {
        var sqlBuilder = Sql.Empty;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
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
            async (viewModel, db) =>
            {
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
    public async Task NotCreateAliasWhenNoUwpAppIsSelectedAndFileNameIsEmpty()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) =>
            {
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
            async (viewModel, db) =>
            {
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
    public async Task NotRecreateAliasOnLoad()
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
            async (viewModel, _) =>
            {
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
    [InlineData(1)]
    [InlineData(2)]
    public async Task RefreshCacheWhenDeleteAliasLogically(int countToAdd)
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) =>
            {
                // Configured to say yes when it'll be asked to delete the alias
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };

        await TestViewModelAsync(
            async (viewModel, _) =>
            {
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
    public async Task UpdateAliasWithLuaScript()
    {
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
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
    }

    #endregion
}