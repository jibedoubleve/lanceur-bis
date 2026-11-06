using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public sealed class AnalyticsViewModelTest : ViewModelTester<AnalyticsViewModel>
{
    #region Constructors

    public AnalyticsViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceHooks? hooks
    )
    {
        serviceCollection.AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    [Fact]
    public void When_selecting_year_with_empty_db_Then_no_error() =>
        TestViewModel(
            (viewModel, _) => {
                Record.Exception(() => viewModel.SelectYearCommand.Execute(null))
                      .ShouldBeNull();
            },
            Sql.Empty
        );

    #endregion
}