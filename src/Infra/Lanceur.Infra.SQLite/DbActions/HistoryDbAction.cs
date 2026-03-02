using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur.Infra.SQLite.DbActions;

internal class HistoryDbAction
{
    #region Fields

    private readonly IDbConnectionManager _db;

    #endregion

    #region Constructors

    internal HistoryDbAction(IDbConnectionManager manager) => _db = manager;

    #endregion

    #region Methods

    private IEnumerable<DataPoint<DateTime, double>> PerDay()
    {
        const string sql = """
                           select
                               day        as X,
                               exec_count as Y
                           from stat_usage_per_day_v;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql));
    }

    private IEnumerable<DataPoint<DateTime, double>> PerDayOfWeek()
    {
        const string sql = """
                           select
                               date('0001-01-'|| printf('%02d', day_of_week)) as X,
                           	   exec_count                                     as Y
                           from stat_usage_per_day_of_week_v;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql));
    }

    private IEnumerable<DataPoint<DateTime, double>> PerHour()
    {
        const string sql = """
                           select
                               hour_in_day as X,
                           	   exec_count  as Y
                           from stat_usage_per_hour_in_day_v;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql));
    }

    private IEnumerable<DataPoint<DateTime, double>> PerMonth()
    {
        const string sql = """
                           select
                               month      as X,
                           	   exec_count as Y
                           from stat_usage_per_month_v;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql));
    }

    private IEnumerable<DataPoint<DateTime, double>> PerYear()
    {
        const string sql = """
                           select
                               year       as X,
                           	   exec_count as Y
                           from stat_usage_per_year_v;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql));
    }

    internal IEnumerable<DataPoint<DateTime, double>> PerDay(int? year)
    {
        if (year is null) { return PerDay(); }

        const string sql = """
                           select
                               strftime('%Y-%m-%d', time_stamp) as X, -- day
                               count(*)                         as Y  -- exec_count
                           from
                               alias_usage
                           group by
                               strftime('%Y-%m-%d', time_stamp)
                           	and strftime('%Y', time_stamp) = @year
                           order by
                               time_stamp,
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql, new { year = $"{year}" }));
    }

    internal IEnumerable<DataPoint<DateTime, double>> PerDayOfWeek(int? year)
    {
        if (year is null) { return PerDayOfWeek(); }

        const string sql = """
                           select
                               date('0001-01-'|| printf('%02d', day_of_week)) as X, -- day_of_week
                               sum(exec_count)  as Y                                --exec_count
                           from (
                                select *
                                from (
                                         select
                                             count(*)   as exec_count,
                                             case cast(strftime('%w', time_stamp) as integer)
                                                 when 0 then 7
                                                 else cast(strftime('%w', time_stamp) as integer)
                                                 end as day_of_week,
                                             case cast(strftime('%w', time_stamp) as integer)
                                                 when 0 then 'Sunday'
                                                 when 1 then 'Monday'
                                                 when 2 then 'Tuesday'
                                                 when 3 then 'Wednesday'
                                                 when 4 then 'Thursday'
                                                 when 5 then 'Friday'
                                                 when 6 then 'Saturday'
                                                 else 'error'
                                                 end as day_name
                                         from
                                             alias_usage
                                       where 
                                           strftime('%Y', time_stamp) = @year
                                         group by
                                             strftime('%w', time_stamp)
                                     )
                                union all
                                select
                                    w.exec_count  as exec_count,
                                    w.day_of_week as day_of_week,
                                    w.day_name    as day_name
                                from
                                    helper_day_in_week w
                                )
                           group by day_of_week
                           order by
                               day_of_week
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql, new { year = $"{year}" }));
    }

    internal IEnumerable<DataPoint<DateTime, double>> PerHour(int? year)
    {
        if (year is null) { return PerHour(); }

        const string sql = """
                           select
                               hour_in_day     as X, -- hour_in_day
                               sum(exec_count) as Y  -- exec_count,
                           from (
                                   select *
                                   from (
                                       select
                                           count(*)                      as exec_count,
                                           strftime('%H:00', time_stamp) as hour_in_day
                                       from
                                           alias_usage
                           			where strftime('%Y', time_stamp) = @year
                                       group by strftime('%H:00', time_stamp)
                                       )
                                   union all
                                   select
                                       h.exec_count  as exec_count,
                                       h.hour_in_day as hour_in_day
                                   from
                                       helper_hour_in_day h
                                )
                           group by hour_in_day
                           order by
                               hour_in_day
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql, new { year = $"{year}" }));
    }

    internal IEnumerable<DataPoint<DateTime, double>> PerMonth(int? year)
    {
        if (year is null) { return PerMonth(); }

        const string sql = """
                           select
                               strftime('%Y-%m-01', time_stamp) as X, -- month
                               count(*)                         as Y  -- exec_count
                           from
                               alias_usage
                           where
                               strftime('%Y-%m-%d', time_stamp) < strftime('%Y-%m-01', date())
                               and strftime('%Y', time_stamp) = @year
                           group by
                               strftime('%Y-%m-01', time_stamp)
                           order by
                               time_stamp
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql, new { year = $"{year}" }));
    }

    internal IEnumerable<DataPoint<DateTime, double>> PerYear(int? year)
    {
        if (year is null) { return PerYear(); }

        const string sql = """
                           select
                               year       as X,
                           	   exec_count as Y
                           from stat_usage_per_year_v
                           where strftime('%Y', year) = @year;
                           """;
        return _db.WithConnection(conn => conn.Query<DataPoint<DateTime, double>>(sql, new { year = $"{year}" }));
    }

    #endregion
}