using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI;
using ReactiveUI.Testing;
using System.Reactive.Concurrency;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public partial class MainViewModelShould : ReactiveTest
    {
        #region Methods

        [Fact]
        public void AddResultsAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>()).Returns(new List<QueryResult> { new NotExecutableTestAlias(), new NotExecutableTestAlias() });
                var vm = MainViewModelHelper.Build(scheduler, searchService);

                vm.SearchAlias.Execute("__").Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(2);
            });
        }

        [Fact]
        public void AddResultsWithParametersAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var results = new List<ExecutableQueryResult> { new ExecutableTestAlias() };
                var store = Substitute.For<ISearchService>();
                store.Search(Arg.Any<Cmdline>()).Returns(results);

                var storeLoader = Substitute.For<IStoreLoader>();
                storeLoader.Load().Returns(new List<ISearchService> { store });

                var macroMgr = Substitute.For<IMacroManager>();
                var thumbnailManager = Substitute.For<IThumbnailManager>();
                macroMgr.Handle(Arg.Any<IEnumerable<QueryResult>>()).Returns(results);
                var searchService = new SearchService(storeLoader, macroMgr, thumbnailManager);
                var vm = MainViewModelHelper.Build(scheduler, searchService, cmdProcessor: new CmdlineProcessor());

                //Act
                var @params = "parameters";
                vm.SearchAlias.Execute("Search " + @params).Subscribe();

                //Assert
                scheduler.AdvanceBy(1_000);
                vm.Results.ElementAt(0)?.Query.Parameters.Should().Be(@params);
            });
        }

        [Fact]
        public void AutoCompleteQueryWithArguments()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler, cmdProcessor: new CmdlineProcessor());
                vm.SetResults(5);
                vm.Query = "1 un";

                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(110).Ticks);

                vm.AutoCompleteQuery.Execute().Subscribe();

                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(110).Ticks);

                vm.Query.Should().Be("1/5 un");
            });
        }

        [Fact]
        public void ExecuteA_NOT_ExecutableQueryResult()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler);

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
                var vm = MainViewModelHelper.Build(scheduler);

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
        public void ExecuteAnExecutableQueryResultWithParameters()
        {
            new TestScheduler().With(scheduler =>
            {
                var searchService = Substitute.For<ISearchService>();
                var vm = MainViewModelHelper.Build(scheduler, searchService, cmdProcessor: new CmdlineProcessor());

                vm.CurrentAlias = new ExecutableWithResultsTestAlias();
                vm.ExecuteAlias.Execute().Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(2);
            });
        }

        [Fact]
        public void ExecuteAnExecutableQueryResultWithResults()
        {
            new TestScheduler().With(scheduler =>
            {
                var searchService = Substitute.For<ISearchService>();
                var vm = MainViewModelHelper.Build(scheduler, searchService, cmdProcessor: new CmdlineProcessor());

                vm.CurrentAlias = new ExecutableWithResultsTestAlias();
                vm.ExecuteAlias.Execute().Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(2);
            });
        }

        [Fact]
        public void ExecuteResultWithParameter()
        {
            new TestScheduler().With(scheduler =>
            {
                var alias = ExecutableTestAlias.FromName("alias1");
                var vm = MainViewModelHelper.Build(scheduler, cmdProcessor: new CmdlineProcessor());
                vm.CurrentAlias = alias;
                vm.ExecuteAlias.Execute("alias1 world").Subscribe();

                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(11).Ticks);

                (vm.CurrentAlias as ExecutableTestAlias)?.Should().Be("world");
            });
        }

        [Fact]
        public void HaveNoResultOnEmptyQuery()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler);

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
                var vm = MainViewModelHelper.Build(scheduler);

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
                var vm = MainViewModelHelper.Build(scheduler);

                vm.SearchAlias.Execute(" ").Subscribe();

                scheduler.AdvanceBy(1_000);

                vm.Results.Count.Should().Be(0);
            });
        }

        [Fact]
        public void NotifyWhenCriterionChanges()
        {
            //https://stackoverflow.com/questions/49338867/unit-testing-viewmodel-property-bound-to-reactivecommand-isexecuting
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler);

                scheduler.Schedule(TimeSpan.FromTicks(00), () => vm.Query = "a");
                scheduler.Schedule(TimeSpan.FromTicks(15), () => vm.Query += "b");
                scheduler.Schedule(TimeSpan.FromTicks(25), () => vm.Query += "c");
                scheduler.Schedule(TimeSpan.FromTicks(35), () => vm.Query += "d");
                scheduler.Schedule(TimeSpan.FromTicks(45), () => vm.Query += "e");

                var results = scheduler.Start(
                    () => vm.WhenAnyValue(x => x.Query),
                    created: 0,
                    subscribed: 1,
                    disposed: 50
                );

                results.Messages.AssertEqual(
                    OnNext(01, "a"),
                    OnNext(15, "ab"),
                    OnNext(25, "abc"),
                    OnNext(35, "abcd"),
                    OnNext(45, "abcde")
                );
            });
        }

        [Fact]
        public void SearchWhenCriterionChanges()
        {
            //https://stackoverflow.com/questions/49338867/unit-testing-viewmodel-property-bound-to-reactivecommand-isexecuting
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler);

                scheduler.Schedule(TimeSpan.FromMilliseconds(00), () => vm.Query = "a");
                scheduler.Schedule(TimeSpan.FromMilliseconds(50), () => vm.Query = "b");
                scheduler.Schedule(TimeSpan.FromMilliseconds(51), () => vm.Query = "b");
                scheduler.Schedule(TimeSpan.FromMilliseconds(52), () => vm.Query = "b");
                scheduler.Schedule(TimeSpan.FromMilliseconds(80), () => vm.Query = "c");

                var results = scheduler.Start(
                    () => vm.SearchAlias.IsExecuting,
                    created: 0,
                    subscribed: 1,
                    disposed: TimeSpan.FromMilliseconds(1000).Ticks);

                results.Messages.AssertEqual(
                    OnNext(1, false),
                    OnNext(TimeSpan.FromMilliseconds(10).Ticks + 2, true),
                    OnNext(TimeSpan.FromMilliseconds(10).Ticks + 4, false),
                    OnNext(TimeSpan.FromMilliseconds(60).Ticks + 1, true),
                    OnNext(TimeSpan.FromMilliseconds(60).Ticks + 3, false),
                    OnNext(TimeSpan.FromMilliseconds(90).Ticks + 1, true),
                    OnNext(TimeSpan.FromMilliseconds(90).Ticks + 3, false)
                );
            });
        }

        [Fact]
        public void SelectFirstAsCurrentResultsAfterSearch()
        {
            new TestScheduler().With(scheduler =>
            {
                var searchService = Substitute.For<ISearchService>();
                searchService.Search(Arg.Any<Cmdline>()).Returns(new List<QueryResult> { new NotExecutableTestAlias(), new NotExecutableTestAlias() });
                var vm = MainViewModelHelper.Build(scheduler, searchService);

                vm.SearchAlias.Execute("__").Subscribe();

                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(11).Ticks);

                vm.CurrentAlias.Should().NotBeNull();
            });
        }

        [Fact]
        public void SelectFirstAsResultsAfterExecutionWithResults()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler, cmdProcessor: new CmdlineProcessor());
                vm.CurrentAlias = ExecutableWithResultsTestAlias.FromName("alias1");
                vm.ExecuteAlias.Execute().Subscribe();

                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(11).Ticks);

                vm.CurrentAlias.Name.Should().Be("name1");
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        #endregion Methods
    }
}