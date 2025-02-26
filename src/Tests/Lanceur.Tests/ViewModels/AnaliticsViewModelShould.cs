using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class AnaliticsViewModelShould : ViewModelTest<AnalyticsViewModel>
{
    #region Constructors

    public AnaliticsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddSingleton<IMappingService, AutoMapperMappingService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    [Fact]
    public async Task NotCrashWhenNoResultRetrievedFromDb()
    {
        await TestViewModel(
            (viewModel, _) =>
            {
                viewModel.SelectYearCommand.Execute(null);
                return Task.CompletedTask;
            },
            SqlBuilder.Empty
        );
    }

    #endregion
}