using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Tests.Logging;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views.Helpers;
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

                var vm = new MainViewModelBuilder()
                        .With(_output)
                        .With(scheduler)
                        .With(searchService)
                        .Build();

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

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(searchService)
                    .Build();

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
                var query = "1 un";
                var searchService = Substitute.For<ISearchService>();
                var results = MainViewModelTestHelper.BuildResults(5);


                searchService
                    .Search(Arg.Any<Cmdline>())
                    .Returns(new List<QueryResult> { results.First() });

                _output.Arrange();
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(searchService)
                    .Build();

                vm.SetResults(results);

                _output.Act();

                vm.Query.Value = query;
                scheduler.Start();

                vm.AutoComplete.Execute().Subscribe();

                scheduler.Start();

                _output.Assert();
                using (new AssertionScope())
                {
                    vm.Results.Should().HaveCount(1);
                    vm.Query.Value.Should().Be("1/5 un");
                }
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

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(executionManager)
                    .Build();

                // ACT
                vm.CurrentAlias = new ExecutableWithResultsTestAlias();
                var request = vm.BuildExecutionRequest("Some query");
                vm.ExecuteAlias.Execute(request).Subscribe();

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
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .Build();

                vm.SearchAlias.Execute("").Subscribe();

                scheduler.Start();

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultOnNullQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .Build();

                vm.SearchAlias.Execute().Subscribe();

                scheduler.Start();

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultOnWhiteSpaceQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .Build();

                vm.SearchAlias.Execute(" ").Subscribe();

                scheduler.Start();

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void HaveNoResultsWhenExecutingEmptyQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .Build();

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

                var vm = new MainViewModelBuilder()
                    .With(_output)
                    .With(scheduler)
                    .With(executor)
                    .Build();

                // ACT
                vm.CurrentAlias = alias;
                var request = vm.BuildExecutionRequest("alias1");
                vm.ExecuteAlias.Execute(request).Subscribe();

                scheduler.Start();

                // ASSERT
                vm.CurrentAlias.Name.Should().Be("world");
            });
        }

        #endregion Methods
    }
}