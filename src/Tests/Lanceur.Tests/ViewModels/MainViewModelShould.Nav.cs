using FluentAssertions;
using Lanceur.Tests.Utils.Builders;
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
                // ARRANGE
                var vm = new MainViewModelBuilder()
                    .With(OutputHelper)
                    .With(scheduler)
                    .Build();

                // ASSERT
                vm.CurrentAlias.Should().BeNull();
            });
        }

        #endregion Methods
    }
}