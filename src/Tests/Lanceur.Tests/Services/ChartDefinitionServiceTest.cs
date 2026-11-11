using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class ChartDefinitionServiceTest(ITestOutputHelper output) : TestBase(output)
{

    #region Methods

    [Fact]
    public void When_loading_chart_definitions_Then_definitions_are_loaded()
    {
        // arrange
        var service = new SQLiteChartDefinitionRepository(
            Substitute.For<IDbConnectionManager>(),
            Substitute.For<ILogger<SQLiteChartDefinitionRepository>>());

        // act
        var definitions = service.FetchChartDefinitions();

        // assert
        definitions.ShouldNotBeEmpty();
    }

    [Fact]
    public void When_retrieving_chart_definitions_Then_definitions_are_retrieved()
    {
        // arrange
        var service = new SQLiteChartDefinitionRepository(
            Substitute.For<IDbConnectionManager>(),
            Substitute.For<ILogger<SQLiteChartDefinitionRepository>>());

        // act
        var definitions = service.FetchChartDefinitions();

        // assert
        definitions.ShouldSatisfyAllConditions(
            d => d.ShouldAllBe(x => x.Id != Guid.Empty),
            d => d.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Sql)),
            d => d.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Title)),
            d => d.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.XKind)),
            d => d.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.XLabel)),
            d => d.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.YLabel))
        );
    }

    [Fact]
    public void When_usage_history_exists_Then_charts_are_fully_populated()
    {
        // arrange
        var logger = Substitute.For<ILogger<SQLiteChartDefinitionRepository>>();
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms("alias")
                                                     .WithUsage(
                                                         DateTime.Now.AddHours(-1),
                                                         DateTime.Now.AddHours(-2),
                                                         DateTime.Now.AddHours(-3))
        ).ToSql();

        var cstring = ConnectionStringFactory.InMemory;
        using var conn = BuildFreshDb(sql, cstring);
        using var scope = new DbSingleConnectionManager(conn);
        var repository = new SQLiteChartDefinitionRepository(scope, logger);
        var service = new ChartDefinitionService(repository);

        // act
        var charts = service.FetchChartDefinitions();

        // assert
        charts.ShouldSatisfyAllConditions(
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.XLabel)),
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.YLabel)),
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Title))
        );
    }

    #endregion
}