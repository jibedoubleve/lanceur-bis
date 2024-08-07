﻿using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Macros.Development;
using Lanceur.Views.Mixins;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Splat;
using System.Reactive.Concurrency;
using FluentAssertions.Extensions;
using Lanceur.Core.Responses;
using Lanceur.Infra.Services;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.Builders;
using Lanceur.Tests.Tooling.ReservedAliases;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public partial class MainViewModelShould : TestBase
{
    #region Constructors

    public MainViewModelShould(ITestOutputHelper output) : base(output) { }

    #endregion Constructors

    #region Methods

    [Theory, InlineData("=8*5", "40")]
    public void HaveResultWhenQueryIsArithmetic(string expression, string result)
    {
        new TestScheduler().With(
            scheduler =>
            {
                // ARRANGE
                Substitute.For<ILoggerFactory>();

                var executor = Substitute.For<IExecutionManager>();
                executor.ExecuteAsync(Arg.Any<ExecutionRequest>())
                        .Returns(new ExecutionResponse { Results = new List<QueryResult>() { new NotExecutableTestAlias() } });

                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(executor)
                         .Build();

                vm.Query.Value = expression;

                // ACT
                var request = new AliasExecutionRequest { Query = expression };
                vm.ExecuteAlias.Execute(request).Subscribe();

                scheduler.Start();
                vm.CurrentAlias?.Name?.Should().Be(result);
            }
        );
    }

    [Fact]
    public void NotifyWhenCriterionChanges()
    {
        //https://stackoverflow.com/questions/49338867/unit-testing-viewmodel-property-bound-to-reactivecommand-isexecuting
        new TestScheduler().With(
            scheduler =>
            {
                var searchService = Substitute.For<IAsyncSearchService>();
                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(searchService)
                         .Build();

                scheduler.Schedule(() => vm.Query.Value = "a");
                scheduler.Schedule(101.Milliseconds(), () => vm.Query.Value += "b");
                scheduler.Schedule(301.Milliseconds(), () => vm.Query.Value += "c");
                scheduler.Schedule(501.Milliseconds(), () => vm.Query.Value += "d");

                scheduler.AdvanceTo(1_000.Milliseconds().Ticks);

                searchService.ReceivedWithAnyArgs(4)
                             .SearchAsync(default);
            }
        );
    }

    [Fact]
    public void SelectFirstAsCurrentResultsAfterSearch()
    {
        new TestScheduler().With(
            scheduler =>
            {
                // ARRANGE
                var searchService = Substitute.For<IAsyncSearchService>();
                searchService.SearchAsync(Arg.Any<Cmdline>()).Returns(new List<QueryResult> { new NotExecutableTestAlias(), new NotExecutableTestAlias() });
                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(searchService)
                         .Build();

                // ACT
                vm.SearchAlias.Execute("__").Subscribe();

                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Should().NotBeNull();
            }
        );
    }

    [Fact]
    public void SelectFirstAsResultsAfterExecutionWithResults()
    {
        var aliasName = "alias1";
        new TestScheduler().With(
            scheduler =>
            {
                // ARRANGE
                var executor = Substitute.For<IExecutionManager>();
                executor.ExecuteAsync(Arg.Any<ExecutionRequest>())
                        .Returns(new ExecutionResponse { Results = new List<QueryResult>() { ExecutableWithResultsTestAlias.FromName(aliasName) } });

                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(executor)
                         .Build();

                // ACT
                vm.CurrentAlias = ExecutableWithResultsTestAlias.FromName("some random name");

                var request = vm.BuildExecutionRequest(aliasName);
                vm.ExecuteAlias.Execute(request).Subscribe();
                scheduler.Start();

                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAlias.Name.Should().Be(aliasName);
            }
        );
    }

    [Fact]
    public void ShowAutoCompleteWhenCallingDebugMacro()
    {
        Locator.CurrentMutable.Register<ICmdlineManager>(() => new CmdlineManager());
        new TestScheduler().With(
            scheduler =>
            {
                // ARRANGE
                var searchService = Substitute.For<IAsyncSearchService>();
                searchService.SearchAsync(Arg.Any<Cmdline>())
                             .Returns(
                                 new List<QueryResult> { new DebugMacro { Name = "debug" } }
                             );

                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(searchService)
                         .With(new DebugMacroExecutor())
                         .Build();

                // ACT
                vm.Query.Value = "random_query";
                scheduler.Start();

                var request = vm.BuildExecutionRequest("random_query");
                vm.ExecuteAlias.Execute(request).Subscribe(); // Execute first result
                scheduler.Start();

                // ASSERT
                using (new AssertionScope())
                {
                    vm.CurrentAlias.Should().NotBeNull();
                    vm.CurrentAlias?.Name.Should().Be("debug all"); // I know the first result in debug is 'debug all'
                }
            }
        );
    }

    [Theory, InlineData(true, 5), InlineData(false, 0)]
    public void ShowResultWhenConfigured(bool showResult, int expectedCount)
    {
        new TestScheduler().With(
            scheduler =>
            {
                // ARRANGE
                var settings = Substitute.For<ISettingsFacade>();
                settings.Application.Returns(new AppConfig { Window = new() { ShowResult = showResult } });

                var searchService = Substitute.For<IAsyncSearchService>();
                searchService.GetAllAsync().Returns(new AliasQueryResult[] { new(), new(), new(), new(), new() });

                var vm = new MainViewModelBuilder()
                         .With(OutputHelper)
                         .With(scheduler)
                         .With(settings)
                         .With(searchService)
                         .Build();

                // ACT
                vm.Activate.Execute().Subscribe();

                // ASSERT
                scheduler.Start();
                vm.Results.Should().HaveCount(expectedCount);
            }
        );
    }

    #endregion Methods
}