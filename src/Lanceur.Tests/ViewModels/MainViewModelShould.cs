﻿using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Services.Config;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Macros;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Splat;
using System.Reactive.Concurrency;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels
{
    public partial class MainViewModelShould : ReactiveTest
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public MainViewModelShould(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        [Theory]
        [InlineData(true, 5)]
        [InlineData(false, 0)]
        public void ShowResultWhenConfigured(bool showResult, int expectedCount)
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var settings = Substitute.For<IAppConfigService>();
                settings.Current.Returns(new AppConfig
                {
                    Window = new WindowSection()
                    {
                        ShowResult = showResult
                    }
                });

                var dataService = Substitute.For<IDataService>();
                dataService.GetAll().Returns(new AliasQueryResult[]
                {
                    new AliasQueryResult(),
                    new AliasQueryResult(),
                    new AliasQueryResult(),
                    new AliasQueryResult(),
                    new AliasQueryResult()
                });

                var vm = new MainViewModelBuilder()
                            .With(_output)
                            .With(scheduler)
                            .With(settings)
                            .With(dataService)
                            .Build();

                // ACT
                vm.Activate.Execute().Subscribe();

                // ASSERT
                scheduler.Start();
                vm.Results.Should().HaveCount(expectedCount);
            });
        }

        [Theory]
        [InlineData("=8*5", "40")]
        public void HaveResultWhenQueryIsArithmetic(string expression, string result)
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var calculator = new CodingSebCalculatorService();
                var log = Substitute.For<IAppLoggerFactory>();

                var executor = Substitute.For<IExecutionManager>();
                executor.ExecuteAsync(Arg.Any<ExecutionRequest>())
                        .Returns(new ExecutionResponse
                        {
                            Results = new List<QueryResult>()
                            {
                                new NotExecutableTestAlias(),
                            }
                        });

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(executor)
                    .Build();

                vm.Query = expression;

                // ACT
                vm.ExecuteAlias.Execute(expression).Subscribe();

                scheduler.Start();
                vm.CurrentAlias?.Name?.Should().Be(result);
            });
        }

        [Fact]
        public void NotifyWhenCriterionChanges()
        {
            //https://stackoverflow.com/questions/49338867/unit-testing-viewmodel-property-bound-to-reactivecommand-isexecuting
            new TestScheduler().With(scheduler =>
            {
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .Build();

                scheduler.Schedule(() => vm.Query = "a");
                scheduler.Schedule(TimeSpan.FromTicks(200), () => vm.Query += "b");
                scheduler.Schedule(TimeSpan.FromTicks(300), () => vm.Query += "c");
                scheduler.Schedule(TimeSpan.FromTicks(400), () => vm.Query += "d");

                var results = scheduler.Start(
                    () => vm.SearchAlias.IsExecuting,
                    created: 0,
                    subscribed: 100,
                    disposed: TimeSpan.FromMilliseconds(1_000).Ticks);

                results.Messages.AssertEqual(
                    OnNext(100, false),
                    OnNext(TimeSpan.FromMilliseconds(100).Ticks + 402, true),
                    OnNext(TimeSpan.FromMilliseconds(100).Ticks + 404, false)
                );
            });
        }

        [Fact]
        public void SelectFirstAsCurrentResultsAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>()).Returns(new List<QueryResult> { new NotExecutableTestAlias(), new NotExecutableTestAlias() });
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(searchService)
                    .Build();

                // ACT
                vm.SearchAlias.Execute("__").Subscribe();

                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Should().NotBeNull();
            });
        }

        [Fact]
        public void SelectFirstAsResultsAfterExecutionWithResults()
        {
            var aliasName = "alias1";
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var executor = Substitute.For<IExecutionManager>();
                executor.ExecuteAsync(Arg.Any<ExecutionRequest>())
                        .Returns(new ExecutionResponse
                        {
                            Results = new List<QueryResult>() {
                                ExecutableWithResultsTestAlias.FromName(aliasName)
                            }
                        });

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(executor)
                    .Build();

                // ACT
                vm.CurrentAlias = ExecutableWithResultsTestAlias.FromName("some random name");

                vm.ExecuteAlias.Execute(aliasName).Subscribe();
                scheduler.Start();

                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAlias.Name.Should().Be(aliasName);
            });
        }

        [Fact]
        public void ShowAutoCompleteWhenCalingDebugMacro()
        {
            Locator.CurrentMutable.Register<ICmdlineManager>(() => new CmdlineManager());
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>())
                        .Returns(
                            new List<QueryResult>()
                            {
                               new DebugMacro(){ Name = "debug" }
                            }
                        );

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(searchService)
                    .With(new DebugMacroExecutor())
                    .Build();

                // ACT
                vm.Query = "random_query";
                scheduler.Start();

                vm.ExecuteAlias.Execute("random_query").Subscribe(); // Execute first result
                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAlias?.Name.Should().Be("debug all"); // I know the first result in debug is 'debug all'
            });
        }

        #endregion Methods
    }
}