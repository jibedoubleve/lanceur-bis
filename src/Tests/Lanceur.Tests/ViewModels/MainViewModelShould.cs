using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
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
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Lanceur.Ui.Core.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class MainViewModelShould : ViewModelTest<MainViewModel>
{
    #region Constructors

    public MainViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddApplicationSettings(
                             stg => visitors?.VisitSettings?.Invoke(stg)
                         )
                         .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                         .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                         .AddSingleton<IMappingService, AutoMapperMappingService>()
                         .AddSingleton<ISearchService, SearchService>()
                         .AddSingleton<IMacroService, MacroService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<IDatabaseConfigurationService>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddMockSingleton<IUserInteractionService>()
                         .AddMockSingleton<IUserNotificationService>()
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
                using var scope = new AssertionScope();
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
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // ARRANGE
                var alias = Substitute.For<ExecutableQueryResult>();
                viewModel.Results.Add(alias);

                // ACT
                viewModel.Query = operation;
                await viewModel.SearchCommand.ExecuteAsync(null);

                // ASSERT done in ServiceCollectionConfigurator
                using var scope = new AssertionScope();
                viewModel.Results.Should().HaveCountGreaterThan(0);
                viewModel.Results.ElementAt(0).Name.Should().Be(result);
            }
        );
    }

    [Fact]
    public async Task BeAbleToSearchAliases()
    {
        // ARRANGE
        var sqlBuilder = new SqlBuilder().AppendAlias(1, cfg: a => a.WithSynonyms("alias1", "alias_1"))
                                         .AppendAlias(2, cfg: a => a.WithSynonyms("alias2", "alias_2"))
                                         .AppendAlias(3, cfg: a => a.WithSynonyms("alias3", "alias_3"));


        await TestViewModelAsync(
            async (viewModel, _) =>
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

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task ShowAllResultsOrNotDependingOnConfiguration(bool showAllResults, int count)
    {
        var builder = new SqlBuilder().AppendAlias(1, cfg: a => a.WithSynonyms("alias1", "alias_1"))
                                      .AppendAlias(2, cfg: a => a.WithSynonyms("alias2", "alias_2"))
                                      .AppendAlias(3, cfg: a => a.WithSynonyms("alias3", "alias_3"));

        var visitors = new ServiceVisitors
        {
            VisitSettings = settings =>
            {
                var application = new DatabaseConfiguration { SearchBox = new() { ShowResult = showAllResults } };
                settings.Application.Returns(application);
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.DisplayResultsIfAllowed();
                viewModel.Results.Should().HaveCountGreaterOrEqualTo(count);
            },
            builder,
            visitors
        );
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ShowLastResultOrNotDependingOnConfiguration(bool expected, bool expected2)
    {
        ISettingsFacade settings = null;
        var visitors = new ServiceVisitors { VisitSettings = s => settings = s };
        TestViewModel(
            (viewModel, _) =>
            {
                using(new AssertionScope())
                {
                    settings.Application.SearchBox.ShowLastQuery = expected;
                    viewModel.ShowLastQuery.Should().Be(expected, "this is the first call");
                    
                    settings.Application.SearchBox.ShowLastQuery = expected2;
                    viewModel.ShowLastQuery.Should().Be(expected2, "this is the second call");
                }
            },
            SqlBuilder.Empty,
            visitors
        );
    }

    #endregion
}