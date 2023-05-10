using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using System.Reactive.Concurrency;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public partial class MainViewModelShould
    {
        #region Methods

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

        #endregion Methods
    }
}