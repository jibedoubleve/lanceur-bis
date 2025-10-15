using Shouldly;
using Lanceur.Core.Mappers;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class AnalyticsViewModelShould : ViewModelTester<AnalyticsViewModel>
{
    #region Constructors

    public AnalyticsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddSingleton<IMappingService, MappingService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    [Fact]
    public void NotCrashWhenNoResultRetrievedFromDb()
    {
        TestViewModel(
            (viewModel, _) =>
            {
                Record.Exception(() => viewModel.SelectYearCommand.Execute(null))
                      .Should().BeNull();
            },
            Sql.Empty
        );
    }

    #endregion
}