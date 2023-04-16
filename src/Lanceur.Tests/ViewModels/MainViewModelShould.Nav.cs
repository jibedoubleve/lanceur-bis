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
                var vm = Builder.BuildMainViewModel(scheduler);

                //Assert
                vm.CurrentAlias.Should().BeNull();
            });
        }

        #endregion Methods
    }
}