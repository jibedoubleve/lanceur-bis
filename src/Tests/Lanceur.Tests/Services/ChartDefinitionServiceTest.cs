using System.Globalization;
using Lanceur.Core.Utils;
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
    private const string HistoryDailyChartId = "16bbc7ea-71ec-4991-a882-53cb85185441";
    private const string HistoryMonthlyChartId = "a05b71e4-cab8-4419-a271-5517754156c7";
    private const string HistoryYearlyChartId = "6023bb84-9c69-4823-a22f-4e0497714c2d";
    private const string HourOfDayChartId = "a76aa15e-47c0-4efa-88ed-d14e3921913e";
    private const string MonthlyAdoptionsChartId = "1422e36c-1fd6-4f66-ba45-8ccfd140651a";
    private const string MonthlyDiversityChartId = "0eaf7d4c-4eda-464d-b8b2-5ee7d80b8c12";

    #endregion

    #region Properties

    public static TheoryData<string> ChartIds
        => [.. BuildChartDefinitionRepository().FetchChartDefinitions().Select(d => d.Id.ToString())];

    /// <summary>
    ///     Charts whose Y is the number of usages, hence share the same filtering rules.
    /// </summary>
    public static TheoryData<string> UsageCountChartIds
        => [HistoryDailyChartId, HistoryMonthlyChartId, HistoryYearlyChartId, DayOfWeekChartId, HourOfDayChartId];

    #endregion

    #region Methods

    private static SQLiteChartDefinitionRepository BuildChartDefinitionRepository()
        => new(
            Substitute.For<IDbConnectionManager>(),
            Substitute.For<ILogger<SQLiteChartDefinitionRepository>>()
        );

    private ChartDefinitionService BuildChartDefinitionService(SqlBuilder sqlBuilder, IConnectionString? connectionString = null)
    {
        var conn = connectionString ?? ConnectionStringFactory.InMemory;
        var scope = GetConnectionManager(sqlBuilder, conn.ToString());
        var repository = new SQLiteChartDefinitionRepository(scope, CreateLogger<SQLiteChartDefinitionRepository>());
        return new ChartDefinitionService(repository);
    }

    private static double Date(string date) => DateTime.Parse(date, CultureInfo.InvariantCulture).ToOADate();

    private static Dictionary<string, object> Filters(long? year = null, long? idAlias = null)
        => new() { ["year"] = year!, ["idalias"] = idAlias! };

    [Fact]
    public void When_alias_is_used_for_the_first_time_Then_it_is_adopted_that_month()
    {
        // arrange
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("alias1")
                                     .WithUsage("2024-03-04 10:00", "2024-04-04 10:00"))
                  .AppendAlias(a => a.WithSynonyms("alias2")
                                     .WithUsage("2024-04-10 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(MonthlyAdoptionsChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2024-03-01"), Date("2024-04-01")]),
            p => p.Select(x => x.Y).ShouldBe([1d, 1d])
        );
    }

    [Fact]
    public void When_alias_is_used_several_times_in_a_month_Then_diversity_counts_it_once()
    {
        // arrange
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("alias1")
                                     .WithUsage("2024-03-04 10:00", "2024-03-05 10:00", "2024-03-06 10:00"))
                  .AppendAlias(a => a.WithSynonyms("alias2")
                                     .WithUsage("2024-03-07 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(MonthlyDiversityChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2024-03-01")]),
            p => p.Select(x => x.Y).ShouldBe([2d])
        );
    }

    [Fact]
    public void When_alias_was_adopted_in_a_previous_year_Then_it_is_not_adopted_again()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2025-06-07 10:00"));
        var service = BuildChartDefinitionService(sql, ConnectionStringFactory.InMemory);

        // act
        var points = service.FetchChartPoints(Guid.Parse(MonthlyAdoptionsChartId), Filters(2025));

        // assert
        points.ShouldBeEmpty();
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

    [Fact]
    public void When_filtering_adoptions_by_year_Then_only_aliases_adopted_that_year_are_shown()
    {
        // arrange
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("alias1")
                                     .WithUsage("2024-03-04 10:00"))
                  .AppendAlias(a => a.WithSynonyms("alias2")
                                     .WithUsage("2025-06-07 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(MonthlyAdoptionsChartId), Filters(2025));

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2025-06-01")]),
            p => p.Select(x => x.Y).ShouldBe([1d])
        );
    }

    [Theory]
    [MemberData(nameof(UsageCountChartIds))]
    public void When_filtering_by_alias_Then_only_usages_of_that_alias_are_counted(string chartId)
    {
        // arrange
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("alias1")
                                     .WithUsage("2024-03-04 10:00", "2024-03-05 10:00"))
                  .AppendAlias(a => a.WithSynonyms("alias2")
                                     .WithUsage("2024-03-04 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(chartId), Filters(idAlias: 2));

        // assert
        points.Sum(p => p.Y).ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(UsageCountChartIds))]
    public void When_filtering_by_year_Then_only_usages_of_that_year_are_counted(string chartId)
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2024-03-05 10:00", "2025-03-04 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(chartId), Filters(2024));

        // assert
        points.Sum(p => p.Y).ShouldBe(2);
    }

    [Fact]
    public void When_filtering_diversity_by_year_Then_only_months_of_that_year_are_shown()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2025-03-04 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(MonthlyDiversityChartId), Filters(2025));

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2025-03-01")]),
            p => p.Select(x => x.Y).ShouldBe([1d])
        );
    }

    [Fact]
    public void When_loading_chart_definitions_Then_definitions_are_loaded()
    {
        // act
        var definitions = BuildChartDefinitionRepository().FetchChartDefinitions();

        // assert
        definitions.ShouldNotBeEmpty();
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
    public void When_showing_daily_history_Then_usages_are_counted_per_day()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2024-03-04 15:00", "2024-03-05 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(HistoryDailyChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2024-03-04"), Date("2024-03-05")]),
            p => p.Select(x => x.Y).ShouldBe([2d, 1d])
        );
    }

    [Fact]
    public void When_showing_monthly_history_Then_usages_are_counted_per_month()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2024-03-28 10:00", "2024-04-01 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(HistoryMonthlyChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2024-03-01"), Date("2024-04-01")]),
            p => p.Select(x => x.Y).ShouldBe([2d, 1d])
        );
    }

    [Fact]
    public void When_showing_usage_by_day_of_week_Then_x_is_the_row_index_and_label_is_the_day()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2024-03-11 10:00", "2024-03-09 10:00")); // Mon, Mon, Sat
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(DayOfWeekChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([0d, 1d]),
            p => p.Select(x => x.Label).ShouldBe(["Monday", "Saturday"]),
            p => p.Select(x => x.Y).ShouldBe([2d, 1d])
        );
    }

    [Fact]
    public void When_showing_usage_by_hour_of_day_Then_usages_are_counted_per_hour()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 08:15", "2024-03-05 08:45", "2024-03-04 14:30"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(HourOfDayChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.Label).ShouldBe(["08:00", "14:00"]),
            p => p.Select(x => x.Y).ShouldBe([2d, 1d])
        );
    }

    [Fact]
    public void When_showing_yearly_history_Then_usages_are_counted_per_year()
    {
        // arrange
        var sql = new SqlBuilder()
            .AppendAlias(a => a.WithSynonyms("alias1")
                               .WithUsage("2024-03-04 10:00", "2024-12-31 10:00", "2025-01-01 10:00"));
        var service = BuildChartDefinitionService(sql);

        // act
        var points = service.FetchChartPoints(Guid.Parse(HistoryYearlyChartId), Filters());

        // assert
        points.ShouldSatisfyAllConditions(
            p => p.Select(x => x.X).ShouldBe([Date("2024-01-01"), Date("2025-01-01")]),
            p => p.Select(x => x.Y).ShouldBe([2d, 1d])
        );
    }

    [Theory]
    [MemberData(nameof(ChartIds))]
    public void When_usage_history_exists_Then_chart_has_points(string chartId)
    {
        // arrange
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("alias1")
                                            .WithUsage("2024-03-04 10:15", "2024-03-04 10:45", "2024-03-05 14:30"))
                         .AppendAlias(a => a.WithSynonyms("alias2")
                                            .WithUsage("2024-03-05 08:00", "2025-06-07 08:00"));
        var service = BuildChartDefinitionService(sqlBuilder);

        // act
        var points = service.FetchChartPoints(Guid.Parse(chartId), Filters());

        // assert
        points.ShouldNotBeEmpty();
    }

    [Fact]
    public void When_usage_history_exists_Then_charts_are_fully_populated()
    {
        // arrange
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("alias1")
                                            .WithUsage("2024-03-04 10:15", "2024-03-04 10:45", "2024-03-05 14:30"))
                         .AppendAlias(a => a.WithSynonyms("alias2")
                                            .WithUsage("2024-03-05 08:00", "2025-06-07 08:00"));
        var service = BuildChartDefinitionService(sqlBuilder);

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
        var sqlBuilder = new SqlBuilder()
                         .AppendAlias(a => a.WithSynonyms("alias1")
                                            .WithUsage("2024-03-04 10:15", "2024-03-04 10:45", "2024-03-05 14:30"))
                         .AppendAlias(a => a.WithSynonyms("alias2")
                                            .WithUsage("2024-03-05 08:00", "2025-06-07 08:00"));
        var service = BuildChartDefinitionService(sqlBuilder);

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
            _ => aliases.Values.Skip(1).Select(Convert.ToInt64).ShouldBe([1L, 2L], true)
        );
    }

    #endregion
}