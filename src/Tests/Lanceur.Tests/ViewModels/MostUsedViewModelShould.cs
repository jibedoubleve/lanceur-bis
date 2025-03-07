using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class MostUsedViewModelShould : ViewModelTest<MostUsedViewModel>
{
    private const int HistorySize = 5;

    #region Constructors

    public MostUsedViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static SqlBuilder BuildSqlBuilder()
    {
        return new SqlBuilder().AppendAlias(
                                   1,
                                   aliasSql: a =>
                                   {
                                       a.WithSynonyms("a");
                                       a.WithUsage(
                                           DateTime.Parse("2025-01-01"),
                                           DateTime.Parse("2025-02-01"),
                                           DateTime.Parse("2025-03-01"),
                                           DateTime.Parse("2021-03-01"),
                                           DateTime.Parse("2022-03-01")
                                       );
                                   }
                               )
                               .AppendAlias(
                                   2,
                                   aliasSql: a =>
                                   {
                                       a.WithSynonyms("b");
                                       a.WithUsage(
                                           DateTime.Parse("2024-01-01"),
                                           DateTime.Parse("2024-02-01"),
                                           DateTime.Parse("2024-03-01"),
                                           DateTime.Parse("2021-03-01"),
                                           DateTime.Parse("2022-03-01")
                                       );
                                   }
                               );
    }

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddSingleton<IMappingService, AutoMapperMappingService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>();
        return serviceCollection;
    }

    [Theory]
    [InlineData("")]
    [InlineData("Get all aliases")]
    [InlineData(null)]
    [InlineData("All")]
    public async Task ShowAllUsageForAll(string filter)
    {
        var visitor  = new ServiceVisitors() { OverridenConnectionString = ConnectionStringFactory.InDesktop };
        var sqlBuilder = BuildSqlBuilder();

        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                // act
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                await viewModel.RefreshAliasesCommand.ExecuteAsync(filter);

                // assert
                using (new AssertionScope())
                {
                    viewModel.Aliases.Should().HaveCount(2);
                    foreach (var alias in viewModel.Aliases) alias.Count.Should().Be(HistorySize);
                }
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
                await viewModel.LoadAliasesCommand.ExecuteAsync(null);
                await viewModel.RefreshAliasesCommand.ExecuteAsync(year);

                // assert 
                using (new AssertionScope())
                {
                    viewModel.Aliases.Should().HaveCount(1);
                    foreach (var alias in viewModel.Aliases) alias.Count.Should().Be(3);
                }
            },
            sqlBuilder,
            visitors
        );
    }

    #endregion
}