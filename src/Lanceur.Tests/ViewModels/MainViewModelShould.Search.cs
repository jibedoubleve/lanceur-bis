using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Tests.Logging;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public partial class MainViewModelShould
    {
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

                var vm = Builder.Build(_output)
                                .With(scheduler)
                                .With(searchService)
                                .BuildMainViewModel();

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
                var results = new List<SelfExecutableQueryResult> { new ExecutableTestAlias() };
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
                    .Build(_output)
                    .With(scheduler)
                    .With(searchService)
                    .BuildMainViewModel();

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
                    .Build(_output)
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
        public void HaveMultipleResultsWhenExecutingAlias()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var executionManager = Substitute.For<IExecutionManager>();
                executionManager
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
                    .Build(_output)
                    .With(scheduler)
                    .With(executionManager)
                    .BuildMainViewModel();

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
                    .Build(_output)
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
                    .Build(_output)
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
                    .Build(_output)
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
                    .Build(_output)
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
                    .Build(_output)
                    .With(scheduler)
                    .With(executor)
                    .BuildMainViewModel();

                // ACT
                vm.CurrentAlias = alias;
                vm.ExecuteAlias.Execute("alias1").Subscribe();

                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Name.Should().Be("world");
            });
        }

        #endregion Methods
    }
}