using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class ChartDefinitionServiceTest(ITestOutputHelper output) : TestBase(output)
{
    #region Fields

    private const string DayOfWeekChartId = "2da41241-66b2-436e-a907-df0e4db4bad8";

    #endregion

    #region Properties

    public static TheoryData<string> ChartIds
        => [.. BuildChartDefinitionRepository().FetchChartDefinitions().Select(d => d.Id.ToString())];

    public static TheoryData<SqlBuilder, string, long?, long?, int> ChartIdsWithFilters
    {
        get
        {
            var data = new TheoryData<SqlBuilder, string, long?, long?, int>();
            
            var sqlBuilder = new SqlBuilder()
                .AppendAlias(a => a.WithSynonyms("alias1")
                                   .WithUsage("2024-03-04 10:15", "2024-03-04 10:45", "2024-03-05 14:30"))
                .AppendAlias(a => a.WithSynonyms("alias2")
                                   .WithUsage("2024-03-05 08:00", "2025-06-07 08:00"));
            
            var chartIds = BuildChartDefinitionRepository()
                      .FetchChartDefinitions()
                      .Select(d => d.Id.ToString());
            
            foreach (var id in chartIds)
            {
                /* chartId, year, idAlias, expectedTotal
                 * That's, chart 'chartId' shows alias 'IdAlias'
                 * has been used 'expectedTotal' times for year
                 * 'year'
                 */
                data.Add(sqlBuilder, id, null, null, 5);
                data.Add(sqlBuilder, id, 2024, null, 4);
                data.Add(sqlBuilder, id, 2025, null, 1);
                data.Add(sqlBuilder, id, null, 1, 3);
                data.Add(sqlBuilder, id, null, 2, 2);
                data.Add(sqlBuilder, id, 2024, 2, 1);
                data.Add(sqlBuilder, id, 2023, null, 0);
            }

            return data;
        }
    }

    #endregion

    #region Methods

    private static Dictionary<string, object> Filters(long? year = null, long? idAlias = null)
        => new() { ["year"] = year!, ["idalias"] = idAlias! };

    private static SQLiteChartDefinitionRepository BuildChartDefinitionRepository()
        => new(
            Substitute.For<IDbConnectionManager>(),
            Substitute.For<ILogger<SQLiteChartDefinitionRepository>>()
        );

    private ChartDefinitionService BuildChartDefinitionService(SqlBuilder sqlBuilder)
    {
        var scope = GetConnectionManager(sqlBuilder);
        var repository = new SQLiteChartDefinitionRepository(scope, CreateLogger<SQLiteChartDefinitionRepository>());
        return new ChartDefinitionService(repository);
    }

    private static SqlBuilder UsageHistory()
        => new SqlBuilder()
           .AppendAlias(a => a.WithSynonyms("alias1")
                              .WithUsage("2024-03-04 10:15", "2024-03-04 10:45", "2024-03-05 14:30"))
           .AppendAlias(a => a.WithSynonyms("alias2")
                              .WithUsage("2024-03-05 08:00", "2025-06-07 08:00"));

    [Fact]
    public void When_loading_chart_definitions_Then_definitions_are_loaded()
    {
        // act
        var definitions = BuildChartDefinitionRepository().FetchChartDefinitions();

        // assert
        definitions.ShouldNotBeEmpty();
    }

    [Fact]
    public void When_loading_chart_definitions_Then_ids_and_orders_are_unique()
    {
        // act
        var definitions = BuildChartDefinitionRepository().FetchChartDefinitions();

        // assert
        definitions.ShouldSatisfyAllConditions(
            d => d.Select(x => x.Id).ShouldBeUnique(),
            d => d.Select(x => x.Order).ShouldBeUnique()
        );
    }

    [Fact]
    public void When_loading_chart_definitions_Then_every_filter_is_used_by_the_chart_query()
    {
        // act
        var definitions = BuildChartDefinitionRepository().FetchChartDefinitions();

        // assert
        definitions.ShouldAllBe(d =>
            d.Filters.All(f => d.Sql.Contains("@" + f.ParamName, StringComparison.OrdinalIgnoreCase))
        );
    }

    [Fact]
    public void When_retrieving_chart_definitions_Then_definitions_are_retrieved()
    {
        // act
        var definitions = BuildChartDefinitionRepository().FetchChartDefinitions();

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
        var service = BuildChartDefinitionService(UsageHistory());

        // act
        var charts = service.FetchChartDefinitions();

        // assert
        charts.ShouldSatisfyAllConditions(
            c => c.ShouldNotBeEmpty(),
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.XLabel)),
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.YLabel)),
            c => c.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Title))
        );
    }

    [Fact]
    public void When_usage_history_exists_Then_filter_options_start_with_all_and_list_the_data()
    {
        // arrange
        var service = BuildChartDefinitionService(UsageHistory());

        // act
        var filters = service.FetchChartDefinitions()
                             .SelectMany(c => c.Filters)
                             .ToArray();

        // assert
        var years = filters.First(f => f.Parameter == "year").Options;
        var aliases = filters.First(f => f.Parameter == "idalias").Options;
        filters.ShouldSatisfyAllConditions(
            f => f.ShouldAllBe(x => x.Options.First().Value == null),
            _ => years.Values.Skip(1).Select(Convert.ToInt64).ShouldBe([2024L, 2025L]),
            _ => aliases.Values.Skip(1).Select(Convert.ToInt64).ShouldBe([1L, 2L], ignoreOrder: true)
        );
    }

    [Theory]
    [MemberData(nameof(ChartIds))]
    public void When_database_is_empty_Then_chart_has_no_points(string chartId)
    {
        // arrange
        var service = BuildChartDefinitionService(new SqlBuilder());

        // act
        var points = service.FetchChartPoints(Guid.Parse(chartId), Filters());

        // assert
        points.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(ChartIdsWithFilters))]
    public void When_filtering_chart_Then_only_matching_usages_are_counted(
        SqlBuilder sql, string chartId, long? year, long? idAlias, int expectedTotal)
    {
        // arrange
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(chartId), Filters(year, idAlias));

        // assert
        points.Sum(p => p.Y).ShouldBe(expectedTotal);
    }

    [Fact]
    public void When_x_kind_is_text_Then_x_is_the_row_index_and_label_is_the_text()
    {
        // arrange
        var service = BuildChartDefinitionService(UsageHistory());

        // act
        var points = service.FetchChartPoints(Guid.Parse(DayOfWeekChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([0d, 1d, 2d]),
            p => p.Select(x => x.Label).ShouldBe(["Monday", "Tuesday", "Saturday"]),
            p => p.Select(x => x.Y).ShouldBe([2d, 2d, 1d])
        );
    }

    #endregion
}