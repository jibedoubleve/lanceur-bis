using System.Reflection;
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
using Lanceur.Infra.Stores;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class MainViewModelShould : TestBase
{
    #region Constructors

    public MainViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private async Task TestViewModel(Func<MainViewModel, Task> scope, SqlBuilder sqlBuilder = null, ServiceVisitors configurator = null)
    {
        using var db = GetDatabase(sqlBuilder ?? SqlBuilder.Empty);
        var serviceCollection = new ServiceCollection().AddView<MainViewModel>()
                                                       .AddLogging(builder => builder.AddXUnit(OutputHelper))
                                                       .AddMemoryDb(db)
                                                       .AddApplicationSettings()
                                                       .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                                                       .AddSingleton<IMappingService, AutoMapperMappingService>()
                                                       .AddSingleton<ISearchService, SearchService>()
                                                       .AddSingleton<IMacroManager, MacroManager>()
                                                       .AddMockSingleton<IAppConfigRepository>()
                                                       .AddMockSingleton<IThumbnailManager>()
                                                       .AddMockSingleton<IUserInteractionService>()
                                                       .AddMockSingleton<IUserNotificationService>()
                                                       .AddSingleton<IWatchdogBuilder, TestWatchdogBuilder>()
                                                       .AddMockSingleton<IExecutionManager>(
                                                           (sp, i) =>
                                                           {
                                                               i.ExecuteAsync(Arg.Any<ExecutionRequest>())
                                                                .Returns(ExecutionResponse.NoResult);
                                                               return configurator?.VisitExecutionManager?.Invoke(sp, i) ?? i;
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
        var viewModel = serviceProvider.GetService<MainViewModel>();
        await scope(viewModel);
    }

    [Fact]
    public async Task BeAbleToExecuteAliases()
    {
        IExecutionManager sut = null;
        var visitors = new ServiceVisitors
        {
            VisitExecutionManager = (_, i) =>
            {
                sut = i;
                return i;
            }
        };
        await TestViewModel(
            async viewModel =>
            {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.SelectedResult = alias;
                await viewModel.ExecuteCommand.ExecuteAsync(false); //RunAsAdmin: false

                // ASSERT done in ServiceCollectionConfigurator
                using var _ = new AssertionScope();
                sut.Should().NotBeNull();
                await sut.Received().ExecuteAsync(Arg.Any<ExecutionRequest>());
            },
            SqlBuilder.Empty,
            visitors
        );
    }

    [Theory]
    [InlineData("2+5", "7")]
    [InlineData("2 + 5", "7")]
    [InlineData("2 * 5", "10")]
    public async Task BeAbleToExecuteCalculation(string operation, string result)
    {
        await TestViewModel(
            async viewModel =>
            {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.Query = operation;
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT done in ServiceCollectionConfigurator
                using var _ = new AssertionScope();
                viewModel.Results.Should().HaveCountGreaterThan(0);
                viewModel.Results.ElementAt(0).Name.Should().Be(result);
            }
        );
    }

    [Fact]
    public async Task BeAbleToSearchAliases()
    {
        // ARRANGE
        var sqlBuilder = new SqlBuilder().AppendAlias(1, synonyms: ["alias1", "alias_1"])
                                         .AppendAlias(2, synonyms: ["alias2", "alias_2"])
                                         .AppendAlias(3, synonyms: ["alias3", "alias_3"]);


        await TestViewModel(
            async viewModel =>
            {
                // ACT
                viewModel.Query = "alias";
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT
                viewModel.Results.Count.Should().Be(6);
            },
            sqlBuilder
        );
    }

    #endregion

    /// <summary>
    /// Manages a collection of visitor functions that allow users to configure 
    /// custom behaviour for the <c>serviceProvider</c> with specific types.
    /// </summary>
    private class ServiceVisitors
    {
        #region Properties

        public Func<IServiceProvider, IExecutionManager, IExecutionManager> VisitExecutionManager { get; set; }

        #endregion
    }
}