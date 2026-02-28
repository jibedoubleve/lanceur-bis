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

public class MostUsedViewModelShould : ViewModelTester<MostUsedViewModel>
{
    #region Fields

    private const int HistorySize = 5;

    #endregion

    #region Constructors

    public MostUsedViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static SqlBuilder BuildSqlBuilder()
    {
        return new SqlBuilder().AppendAlias(a =>
                                   a.WithSynonyms("a")
                                    .WithUsage(
                                        "2025-01-01",
                                        "2025-02-01",
                                        "2025-03-01",
                                        "2021-03-01",
                                        "2022-03-01"
                                    )
                               )
                               .AppendAlias(a =>
                                   a.WithSynonyms("b")
                                    .WithUsage(
                                        "2024-01-01",
                                        "2024-02-01",
                                        "2024-03-01",
                                        "2021-03-01",
                                        "2022-03-01"
                                    )
                               )
                               .AppendAlias(a => a.WithSynonyms("c"));
    }

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors visitors
    )
    {
        serviceCollection.AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IMemoryCache, MemoryCache>();
        return serviceCollection;
    }

    [Theory]
    [InlineData("")]
    [InlineData("Get all aliases")]
    [InlineData(null)]
    [InlineData("All")]
    public async Task ShowAllUsageForAll(string filter)
    {
        var visitor  = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        var sqlBuilder = BuildSqlBuilder();

        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // act
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                await viewModel.RefreshAliasesCommand.ExecuteAsync(filter);

                // assert
                viewModel.Aliases.Count.ShouldBe(2);
                Assert.All(viewModel.Aliases, alias => alias.Count.ShouldBe(HistorySize));
            },
            sqlBuilder,
            visitor
        );
    }

    [Theory]
    [InlineData("2025")]
    [InlineData("2024")]
    public async Task ShowAllUsageForYear(string year)
    {
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        var sqlBuilder = BuildSqlBuilder();

        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // act
                viewModel.SelectedYear = $"{year}";
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                await viewModel.RefreshAliasesCommand.ExecuteAsync(null);

                // assert 
                viewModel.Aliases.Count.ShouldBe(1);
                Assert.All(viewModel.Aliases, alias =>  alias.Count.ShouldBe(3));
            },
            sqlBuilder,
            visitors
        );
    }

    #endregion
}