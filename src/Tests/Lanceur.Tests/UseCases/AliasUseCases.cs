using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.StateTesters;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.UseCases;

public sealed class AliasUseCases : TestBase
{
    #region Constructors

    public AliasUseCases(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private IServiceProvider BuildServiceProvider()
    {
        var connectionString = ConnectionStringFactory.InMemory;
        var db = GetConnectionManager(Sql.Empty, connectionString.ToString());
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddConfigurationSections()
                         .AddLogging(builder => builder.AddXUnit(OutputHelper))
                         .AddDatabase(db)
                         .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                         .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                         .AddSingleton<ISearchService, SearchService>()
                         .AddSingleton<IMacroAliasExpanderService, MacroAliasExpanderService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IUserCommunicationService>()
                         .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                         .AddMockSingleton<IExecutionService>((_, i) => {
                                 i.ExecuteAsync(Arg.Any<ExecutionRequest>())
                                  .Returns(ExecutionResponse.NoResult);
                                 return i;
                             }
                         )
                         .AddMockSingleton<ISearchServiceOrchestrator>((_, i) => {
                                 i.IsAlive(Arg.Any<IStoreService>(), Arg.Any<Cmdline>())
                                  .Returns(true);
                                 return i;
                             }
                         )
                         .AddSingleton<IAliasManagementService, AliasManagementService>()
                         .AddSingleton<IAliasValidationService, AliasValidationService>()
                         .AddMockSingleton<IViewFactory>()
                         .AddMockSingleton<IUserDialogueService>((_, i) => {
                                 i.AskUserYesNoAsync(Arg.Any<string>())
                                  .Returns(true);
                                 return i;
                             }
                         )
                         .AddMockSingleton<IPackagedAppSearchService>()
                         .AddSingleton<KeywordsViewModel>()
                         .AddSingleton<MainViewModel>();

        // Register stores
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, AliasStore>());

        return serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task When_creating_and_updating_alias_Then_alias_is_created_and_updated()
    {
        /*
         * Arrange
         */
        var serviceProvider = BuildServiceProvider();
        var keywordsViewModel = serviceProvider.GetService<KeywordsViewModel>()!;
        var mainViewModel = serviceProvider.GetService<MainViewModel>()!;

        /*
         * Act & Assert
         */

        /********************************/
        OutputHelper.Title("1: Create a new alias and save it");
        /********************************/
        keywordsViewModel.CreateAliasCommand.Execute(null);
        var newAlias = keywordsViewModel.SelectedAlias;

        if (newAlias is null) { Assert.Fail("A default alias should be selected when creating a new alias"); }

        newAlias.SetRandomState();
        await keywordsViewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

        /********************************/
        OutputHelper.Title("2: Search the alias and find it");
        /********************************/
        mainViewModel.Query = newAlias.Name;
        await mainViewModel.SearchCommand.ExecuteAsync(null);

        var current = mainViewModel.Results[0];
        mainViewModel.Results!.Count.ShouldBe(1);

        OutputHelper.WriteLine($"Type of first element in results is '{current.GetType()}'");
        OutputHelper.WriteAlias(current);

        Assert.Multiple(
            () => mainViewModel.Results.ShouldNotBeNull(),
            () => current.ShouldBeOfType<AliasQueryResult>(),
            () => newAlias.ShouldHaveState(current)
        );

        /********************************/
        OutputHelper.Title("3: Update the alias and save it");
        /********************************/
        keywordsViewModel.SearchCommand.Execute(mainViewModel.SelectedResult!.Name);
        var aliasToUpdate = keywordsViewModel.SelectedAlias;

        aliasToUpdate.ShouldNotBeNull();

        var state = AliasBuilder.BuildRandomAlias();
        aliasToUpdate.CopyState(state);

        await keywordsViewModel.SaveCurrentAliasCommand.ExecuteAsync(null);

        /********************************/
        OutputHelper.Title("4: Search the alias and check the values are updated");
        /********************************/
        mainViewModel.Query = state.Name;
        await mainViewModel.SearchCommand.ExecuteAsync(null);

        Assert.Multiple(
            () => keywordsViewModel.SelectedAlias.ShouldNotBeNull(),
            () => keywordsViewModel.SelectedAlias!.ShouldHaveState(state),
            () => mainViewModel.SelectedResult.ShouldNotBeNull(),
            () => mainViewModel.SelectedResult.ShouldBeOfType<AliasQueryResult>(),
            () => mainViewModel.SelectedResult.ShouldHaveState(state)
        );
    }

    #endregion
}