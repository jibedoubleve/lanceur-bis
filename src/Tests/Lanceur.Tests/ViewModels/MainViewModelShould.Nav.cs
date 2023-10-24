using FluentAssertions;
using Lanceur.Tests.Utils;
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
                    .With(_output)
                    .With(scheduler)
                    .Build();

                // ASSERT
                vm.CurrentAlias.Should().BeNull();
            });
        }

        #endregion Methods
    }
}