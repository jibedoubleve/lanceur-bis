using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public sealed class UsageCalendarViewModelTest : ViewModelTester<UsageCalendarViewModel>
{
    #region Constructors

    public UsageCalendarViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceHooks? hooks
    )
    {
        serviceCollection.AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddMockSingleton<IThumbnailService>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    [Fact]
    public void When_month_has_no_history_Then_getting_history_of_month_does_not_throw() =>
        TestViewModel((viewModel, _) => {
                // act
                var act = () => viewModel.GetHistoryOfMonth(DateTime.Today);

                // assert
                var result = act.ShouldNotThrow();
                result.Days.ShouldBeEmpty();
            }
        );

    #endregion
}
