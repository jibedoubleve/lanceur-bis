using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Tests.Utils;
using Lanceur.Views;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Splat;
using System.Reactive.Concurrency;
using System.Windows.Controls;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public partial class MainViewModelShould
    {
        #region Methods

        [Fact]
        public void HaveNullCurrentQueryResultByDefault()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                var vm = Builder
                    .Build(_output)
                    .With(scheduler)
                    .BuildMainViewModel();

                // ASSERT
                vm.CurrentAlias.Should().BeNull();
            });
        }

        [Fact]
        public void NavigateToSetupWithoutError()
        {
            new TestScheduler().With(scheduler =>
            {
                // ARRANGE
                Locator.CurrentMutable.Register<ISchedulerProvider>(() => new TestSchedulerProvider(scheduler));
                var searchService = Substitute.For<ISearchService>();


                var vm = Builder
                        .Build(_output)
                        .UseLocator()
                        .With(searchService)
                        .BuildMainViewModel();

                // ACT

                var act = () =>
                {
                    vm.Query = "setup";
                    vm.ExecuteAlias.Execute().Subscribe();
                };
                scheduler.Start();

                // ASSERT
                act.Should().NotThrow();


            });
        }

        #endregion Methods
    }
}