using FluentAssertions;
using Lanceur.Tests.Utils;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
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
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);

                //Assert
                vm.CurrentAlias.Should().BeNull();
            });
        }

        [Fact]
        public void SelectDefaultResults()
        {
            new TestScheduler().With(scheduler =>
            {
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(10);

                vm.CurrentAlias.Should().NotBeNull();
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        [Fact]
        public void SelectIndexZeroWhenNull()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(10);
                vm.CurrentAliasIndex = -1;

                //Act
                vm.SelectNextResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        [InlineData(3, 4)]
        [InlineData(4, 5)]
        [InlineData(5, 6)]
        [InlineData(6, 7)]
        [InlineData(7, 8)]
        [InlineData(8, 9)]
        [InlineData(9, 0)]
        public void SelectNextResult(int actual, int expected)
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(10);
                vm.CurrentAliasIndex = actual;

                //Act
                vm.SelectNextResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(expected);
            });
        }

        [Fact]
        public void SelectNextResultOnEmptyResult()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(0);
                vm.CurrentAliasIndex = 0;

                //Act
                vm.SelectNextResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        [Fact]
        public void SelectNextResultOnUniqueResult()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(1);
                vm.CurrentAliasIndex = 0;

                //Act
                vm.SelectNextResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        [Theory]
        [InlineData(9, 8)]
        [InlineData(8, 7)]
        [InlineData(7, 6)]
        [InlineData(6, 5)]
        [InlineData(5, 4)]
        [InlineData(4, 3)]
        [InlineData(3, 2)]
        [InlineData(2, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 9)]
        public void SelectPreviousResult(int actual, int expected)
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(10);
                vm.CurrentAliasIndex = actual;

                //Act
                vm.SelectPreviousResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(expected);
            });
        }

        [Fact]
        public void SelectPreviousResultOnEmptyResult()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(0);
                vm.CurrentAliasIndex = 0;

                //Act
                vm.SelectPreviousResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }

        [Fact]
        public void SelectPreviousResultOnUniqueResult()
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(1);
                vm.CurrentAliasIndex = 0;

                //Act
                vm.SelectPreviousResult.Execute().Subscribe();

                //Assert
                vm.CurrentAliasIndex.Should().Be(0);
            });
        }
        [Theory]
        [InlineData(0, "1/10")]
        [InlineData(1, "2/10")]
        [InlineData(2, "3/10")]
        [InlineData(3, "4/10")]
        [InlineData(4, "5/10")]
        [InlineData(5, "6/10")]
        [InlineData(6, "7/10")]
        [InlineData(7, "8/10")]
        [InlineData(8, "9/10")]
        [InlineData(9, "10/10")]
        public void SelectResultsWhenSelectingIndex(int index, string expected)
        {
            new TestScheduler().With(scheduler =>
            {
                //Arrange
                var vm = MainViewModelHelper.Build(scheduler);
                vm.SetResults(10);
                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(11).Ticks);

                //Act
                vm.CurrentAliasIndex = index;
                scheduler.AdvanceBy(TimeSpan.FromMilliseconds(11).Ticks);

                //Assert
                vm.CurrentAlias.Name.Should().Be(expected);
            });
        }

        #endregion Methods
    }
}