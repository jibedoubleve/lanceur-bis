using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Macros;
using Lanceur.ReservedKeywords;
using Lanceur.Tests.Logging;
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

        [Fact]
        public void AddResultsAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                _output.Arrange();
                var searchService = Substitute.For<ISearchService>();
                searchService
                    .Search(Arg.Any<Cmdline>())
                    .Returns(new List<QueryResult> { new NotExecutableTestAlias(), new NotExecutableTestAlias() });

                var vm = Builder.With(_output)
                                .With(scheduler)
                                .BuildMainViewModel(searchService);

                _output.Act();
                vm.SearchAlias.Execute("...").Subscribe();
                scheduler.Start();

                _output.Assert();
                vm.Results.Count.Should().Be(2);
            });
        }

        [Fact]
        public void AddResultsWithParametersAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                _output.Arrange();
                var results = new List<ExecutableQueryResult> { new ExecutableTestAlias() };
                var store = Substitute.For<ISearchService>();
                store.Search(Arg.Any<Cmdline>())
                     .Returns(results);

                var storeLoader = Substitute.For<IStoreLoader>();
                storeLoader.Load().Returns(new List<ISearchService> { store });

                var macroMgr = Substitute.For<IMacroManager>();
                macroMgr.Handle(Arg.Any<IEnumerable<QueryResult>>()).Returns(results);

                var thumbnailManager = Substitute.For<IThumbnailManager>();
                var searchService = new SearchService(storeLoader, macroMgr, thumbnailManager);

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(searchService);

                //Act
                _output.Act();
                var @params = "parameters";
                vm.SearchAlias.Execute("Search " + @params).Subscribe();
                scheduler.Start();

                //Assert
                _output.Assert();
                vm.Results.Count.Should().BeGreaterThan(0);
                vm.Results.ElementAt(0)?.Query.Parameters.Should().Be(@params);
            });
        }

        [Fact]
        public void AutoCompleteQueryWithArguments()
        {
            new TestScheduler().With(scheduler =>
            {
                _output.Arrange();
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                vm.SetResults(5);
                vm.Query = "1 un";

                _output.Act();
                vm.AutoComplete.Execute().Subscribe();

                scheduler.Start();
                _output.Assert();
                vm.Query.Should().Be("1/5 un");
            });
        }

        [Fact]
        public void ExecuteA_NOT_ExecutableQueryResult()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                scheduler.Schedule(TimeSpan.FromTicks(00), () => vm.CurrentAlias = new NotExecutableTestAlias());

                var results = scheduler.Start(
                    () => vm.ExecuteAlias.CanExecute,
                    created: 0,
                    subscribed: 1,
                    disposed: 50
                );

                results.Messages.AssertEqual(
                    OnNext(01, false)
                );
            });
        }

        [Fact]
        public void ExecuteAnExecutableQueryResult()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                scheduler.Schedule(TimeSpan.FromTicks(00), () => vm.CurrentAlias = new ExecutableTestAlias());

                var results = scheduler.Start(
                    () => vm.ExecuteAlias.CanExecute,
                    created: 0,
                    subscribed: 1,
                    disposed: 50
                );

                results.Messages.AssertEqual(
                    OnNext(01, true)
                );
            });
        }

        [Fact]
        public void ExecuteSelectedAliasWhenGoToNext()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var names = new string[] { "Alias_1", "Alias_2", "Alias_3", "Alias_4", };
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>())
                        .Returns(
                            new List<QueryResult>()
                            {
                                ExecutableTestAlias.FromName(names[0]),
                                ExecutableTestAlias.FromName(names[1]),
                                ExecutableTestAlias.FromName(names[2]),
                                ExecutableTestAlias.FromName(names[3]),
                            },
                            new List<QueryResult>()
                            {
                                ExecutableTestAlias.FromName(names[1]),
                            }
                        );

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(searchService: searchService);

                // ACT
                vm.Query = "random_query";
                scheduler.Start();

                vm.SelectNextResult?.Execute().Subscribe();
                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAlias.Name.Should().Be(names[1]);
            });
        }

        [Fact]
        public void ExecuteSelectedAliasWhenGoToPrevious()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var names = new string[] { "Alias_1", "Alias_2", "Alias_3", "Alias_4", };
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>())
                        .Returns(
                            new List<QueryResult>()
                            {
                                ExecutableTestAlias.FromName(names[0]),
                                ExecutableTestAlias.FromName(names[1]),
                                ExecutableTestAlias.FromName(names[2]),
                                ExecutableTestAlias.FromName(names[3]),
                            },
                            new List<QueryResult>()
                            {
                                ExecutableTestAlias.FromName(names[3]),
                            }
                        );

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(searchService: searchService);

                // ACT
                vm.Query = "random_query";
                scheduler.Start();

                vm.SelectPreviousResult?.Execute().Subscribe();
                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAlias.Name.Should().Be(names[3]);
            });
        }

        [Fact]
        public void HaveMultipleResultsWhenExecutingAlias()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var searchService = Substitute.For<IExecutionManager>();
                searchService
                    .ExecuteAsync(Arg.Any<ExecutionRequest>())
                    .Returns(new ExecutionResponse
                    {
                        Results = new List<QueryResult>
                        {
                            ExecutableTestAlias.Random(),
                            ExecutableTestAlias.Random(),
                        },
                        HasResult = true
                    });

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(executor: searchService);

                // ACT
                vm.CurrentAlias = new ExecutableWithResultsTestAlias();
                vm.ExecuteAlias.Execute("Some query").Subscribe();

                scheduler.Start();

                // ASSERT
                vm.Results.Count.Should().Be(2);
            });
        }

        [Fact]
        public void HaveNoResultOnEmptyQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                vm.SearchAlias.Execute("").Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultOnNullQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                vm.SearchAlias.Execute().Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultOnWhiteSpaceQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                vm.SearchAlias.Execute(" ").Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultsWhenExecutingEmptyQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                // ACT
                vm.CurrentAlias = new ExecutableWithResultsTestAlias();
                vm.ExecuteAlias.Execute().Subscribe();

                scheduler.Start();

                // ASSERT
                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveResultWhenExecutingAlias()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var alias = ExecutableTestAlias.FromName("alias1");

                var executor = Substitute.For<IExecutionManager>();
                executor
                    .ExecuteAsync(Arg.Any<ExecutionRequest>())
                    .Returns(new ExecutionResponse
                    {
                        Results = new List<QueryResult> { ExecutableTestAlias.FromName("world") },
                        HasResult = true
                    });

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(executor: executor);

                // ACT
                vm.CurrentAlias = alias;
                vm.ExecuteAlias.Execute("alias1").Subscribe();

                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Name.Should().Be("world");
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

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(executor: executor);

                vm.Query = expression;
                vm.CurrentAlias = new CalculatorAlias(calculator, log);

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
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

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
                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(searchService);

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

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(executor: executor);

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

                var vm = Builder
                    .With(_output)
                    .With(scheduler)
                    .BuildMainViewModel(
                        searchService: searchService,
                        executor: new DebugMacroExecutor()
                    );

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