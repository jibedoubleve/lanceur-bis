using System.Reflection;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Tests.ViewModels.Helpers;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class KeywordsViewModelShould : TestBase
{
    #region Constructors

    public KeywordsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private async Task TestViewModel(Func<KeywordsViewModel, IDbConnectionManager, Task> scope, SqlBuilder sqlBuilder = null, ServiceVisitors visitors = null)
    {
        var connectionString = visitors?.OverridenConnectionString ??  ConnectionStringFactory.InMemory;
        using var db = GetDatabase(sqlBuilder ?? SqlBuilder.Empty, connectionString.ToString());
        var serviceCollection = new ServiceCollection().AddView<KeywordsViewModel>()
                                                       .AddLogging(builder => builder.AddXUnit(OutputHelper))
                                                       .AddDatabase(db)
                                                       .AddApplicationSettings(
                                                           stg => visitors?.VisitSettings?.Invoke(stg)
                                                       )
                                                       .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                       .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                                                       .AddSingleton<IMappingService, AutoMapperMappingService>()
                                                       .AddSingleton<ISearchService, SearchService>()
                                                       .AddSingleton<IMacroService, MacroService>()
                                                       .AddSingleton<IDbActionFactory, DbActionFactory>()
                                                       .AddMockSingleton<IDatabaseConfigurationService>()
                                                       .AddSingleton<IAliasManagementService, AliasManagementService>()
                                                       .AddSingleton<IAliasValidationService, AliasValidationService>()
                                                       .AddMockSingleton<IViewFactory>()
                                                       .AddMockSingleton<IPackagedAppSearchService>()
                                                       .AddMockSingleton<IThumbnailService>()
                                                       .AddMockSingleton<IUserInteractionService>(
                                                           (sp, i) => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                                                       )
                                                       .AddMockSingleton<IUserNotificationService>(
                                                           (sp, i) => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                                                       )
                                                       .AddMockSingleton<IUserInteractionHub>()
                                                       .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                                                       .AddSingleton<IMemoryCache, MemoryCache>()
                                                       .AddMockSingleton<IExecutionService>(
                                                           (sp, i) =>
                                                           {
                                                               i.ExecuteAsync(Arg.Any<ExecutionRequest>())
                                                                .Returns(ExecutionResponse.NoResult);
                                                               return visitors?.VisitExecutionManager?.Invoke(sp, i) ?? i;
                                                           }
                                                       )
                                                       .AddMockSingleton<ISearchServiceOrchestrator>(
                                                           (_, i) =>
                                                           {
                                                               i.IsAlive(Arg.Any<IStoreService>(), Arg.Any<Cmdline>())
                                                                .Returns(true);
                                                               return i;
                                                           }
                                                       )
                                                       .AddMockSingleton<IStoreLoader>(
                                                           (sp, i) =>
                                                           {
                                                               i.Load().Returns([new AliasStore(sp), new ReservedAliasStore(sp), new CalculatorStore(sp)]);
                                                               return i;
                                                           }
                                                       );
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var viewModel = serviceProvider.GetService<KeywordsViewModel>();
        await scope(viewModel, db);
    }

    [Fact]
    public async Task CreateAliasWithLuaScript()
    {
        await TestViewModel(
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
                                       a.lua_script as Field
                                   from 
                                       alias a
                                       inner join alias_name an on a.id = an.id_alias
                                   """;
                var res = db.WithConnection(c => c.Query<DynamicAlias<string>>(sql))
                            .ToArray();

                res.Length.Should().Be(1);
                var alias = res.First();
                alias.Field.Should().Be(script);
            },
            SqlBuilder.Empty
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
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns(true);
                return i;
            }
        };

        await TestViewModel(
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
                                       a.Id as Id,
                                   	    an.name as Name,
                                   	    case
                                   		    when a.deleted_at is null then false
                                   		    else true
                                   	    end as Field
                                   from alias a 
                                   inner join alias_name an on a.id = an.id_alias 
                                   """;
                var result = db.WithConnection(c => c.Query<DynamicAlias<bool>>(sql, new { name = (string[]) [name] }))
                               .ToArray();

                using (new AssertionScope())
                {
                    result.Length.Should().BeGreaterThan(0);

                    var alias = result.First();
                    alias.Id.Should().BeGreaterThan(0);
                    alias.Name.Should().Be(name);
                    alias.Field.Should().BeTrue();
                }
            },
            SqlBuilder.Empty,
            visitors
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
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns(true);
                return i;
            }
        };

        await TestViewModel(
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
                                   	    end as Field
                                   from alias a 
                                   inner join alias_name an on a.id = an.id_alias 
                                   --where an.Name = @name;
                                   """;
                var result = db.WithConnection(c => c.Query<DynamicAlias<bool>>(sql, new { name = (string[]) [name] }));
                result = result.ToArray();

                using (new AssertionScope())
                {
                    // This is the warning saying the alias name is already used for a deleted alias
                    userNotificationService.Received().Warn(Arg.Any<string>(), Arg.Any<string>());

                    result.Should().HaveCountGreaterThan(0);
                    var alias = result.First();
                    alias.Id.Should().BeGreaterThan(0);
                    alias.Name.Should().Be(name);
                    alias.Field.Should().BeTrue();
                }
            },
            SqlBuilder.Empty,
            visitors
        );
    }

    [Fact]
    public async Task UpdateAliasWithLuaScript()
    {
        await TestViewModel(
            async (viewModel, db) =>
            {
                // ARRANGE
                const string name = "SomeTestName";
                const string script = "some random text that represent a lua script";
                const string script2 = "7a3f4c8f-9142-44f0-8455-6818e11845c3";

                // ACT
                await viewModel.CreateNewAlias(name, luaScript: script);

                viewModel.SelectedAlias.Should().NotBeNull("it is already selected for update");
                viewModel.SelectedAlias!.LuaScript = script2;
                await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

                //ASSERT
                const string sql = """
                                   select 
                                       a.id         as Id,
                                       an.name      as Name,
                                       a.lua_script as Field
                                   from 
                                       alias a
                                       inner join alias_name an on a.id = an.id_alias
                                   """;
                var res = db.WithConnection(c => c.Query<DynamicAlias<string>>(sql))
                            .ToArray();

                res.Length.Should().Be(1);
                var alias = res.First();
                alias.Field.Should().Be(script2);
            },
            SqlBuilder.Empty
        );
    }

    #endregion
}