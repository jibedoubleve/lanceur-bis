using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Tests._Tooling.StateTesters;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.UseCases;

public class CreateAndUpdateAliasUseCase : TestBase
{
    #region Constructors

    public CreateAndUpdateAliasUseCase(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private IServiceProvider BuildServiceProvider()
    {
        var connectionString = ConnectionStringFactory.InMemory;
        var db = GetDatabase(SqlBuilder.Empty, connectionString.ToString());

        return new ServiceCollection().AddLogging(builder => builder.AddXUnit(OutputHelper))
                                      .AddDatabase(db)
                                      .AddApplicationSettings()
                                      .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                                      .AddSingleton<IMappingService, AutoMapperMappingService>()
                                      .AddSingleton<ISearchService, SearchService>()
                                      .AddSingleton<IMacroService, MacroService>()
                                      .AddSingleton<IDbActionFactory, DbActionFactory>()
                                      .AddMockSingleton<IDatabaseConfigurationService>()
                                      .AddMockSingleton<IThumbnailService>()
                                      .AddMockSingleton<IUserNotificationService>()
                                      .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                                      .AddSingleton<IMemoryCache, MemoryCache>()
                                      .AddMockSingleton<IExecutionService>(
                                          (_, i) =>
                                          {
                                              i.ExecuteAsync(Arg.Any<ExecutionRequest>())
                                               .Returns(ExecutionResponse.NoResult);
                                              return i;
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
                                              i.Load().Returns([new AliasStore(sp)]);
                                              return i;
                                          }
                                      )
                                      .AddSingleton<IAliasManagementService, AliasManagementService>()
                                      .AddSingleton<IAliasValidationService, AliasValidationService>()
                                      .AddMockSingleton<IViewFactory>()
                                      .AddMockSingleton<IUserInteractionService>(
                                          (_, i) =>
                                          {
                                              i.AskUserYesNoAsync(Arg.Any<string>())
                                               .Returns(true);
                                              return i;
                                          }
                                      )
                                      .AddMockSingleton<IPackagedAppSearchService>()
                                      .AddView<KeywordsViewModel>()
                                      .AddView<MainViewModel>()
                                      .BuildServiceProvider();
    }

    [Fact]
    public async Task CreateAndUpdateALias()
    {
        /*
         * Arrange
         */
        var serviceProvider = BuildServiceProvider();
        var keywordsViewModel = serviceProvider.GetService<KeywordsViewModel>();
        var mainViewModel = serviceProvider.GetService<MainViewModel>();

        /*
         * Act & Assert
         */

        /********************************/
        OutputHelper.Title("1: Create a new alias and save it");
        /********************************/
        keywordsViewModel.CreateAliasCommand.Execute(null);
        var newAlias = keywordsViewModel.SelectedAlias;

        if (newAlias is null) Assert.Fail("A default alias should be selected when creating a new alias");

        var stateTester = new AliasStateTester();
        stateTester.UpdateValues(ref newAlias);
        await keywordsViewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

        /********************************/
        OutputHelper.Title("2: Search the alias and find it");
        /********************************/
        mainViewModel.Query = stateTester.Name;
        await mainViewModel.SearchCommand.ExecuteAsync(null);

        using (new AssertionScope())
        {
            mainViewModel.Results.Should().NotBeNull();
            mainViewModel.Results!.Count.Should().BeGreaterThan(0);
            var current = mainViewModel.Results![0];
            stateTester.AssertValues(current as AliasQueryResult);
        }

        /********************************/
        OutputHelper.Title("3: Update the alias and save it");
        /********************************/
        keywordsViewModel.SearchCommand.Execute(mainViewModel.SelectedResult!.Name);
        var aliasToUpdate = keywordsViewModel.SelectedAlias;

        var stateTester2 = new AliasStateTester();
        stateTester2.UpdateValues(ref aliasToUpdate);

        await keywordsViewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

        /********************************/
        OutputHelper.Title("4: Search the alias and check the values are updated");
        /********************************/
        mainViewModel.Query = stateTester.Name;
        await mainViewModel.SearchCommand.ExecuteAsync(null);

        using (new AssertionScope())
        {
            mainViewModel.SelectedResult.Should().NotBeNull();
            stateTester2.AssertValues(keywordsViewModel.SelectedAlias);
        }
    }

    #endregion
}