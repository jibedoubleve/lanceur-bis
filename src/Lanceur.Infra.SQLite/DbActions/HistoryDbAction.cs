using Dapper;
using Lanceur.Core.Models;

namespace Lanceur.Infra.SQLite.DbActions
{
    public class HistoryDbAction
    {
        #region Fields

        private readonly SQLiteConnectionScope _db;

        #endregion Fields

        #region Constructors

        public HistoryDbAction(SQLiteConnectionScope scope)
        {
            _db = scope;
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<DataPoint<DateTime, double>> PerDay(long idSession)
        {
            var sql = @"
                select
                    day        as X,
	                exec_count as Y
                from stat_usage_per_day_v
                where id_session = @idSession;";
            return _db.Connection.Query<DataPoint<DateTime, double>>(sql, new { idSession });
        }

        public IEnumerable<DataPoint<DateTime, double>> PerDayOfWeek(long idSession)
        {
            var sql = @"
                select
                	date('0001-01-'|| printf('%02d', day_of_week)) as X,                    
	                exec_count                                     as Y
                from stat_usage_per_day_of_week_v
                where id_session = @idSession;";
            return _db.Connection.Query<DataPoint<DateTime, double>>(sql, new { idSession });
        }

        public IEnumerable<DataPoint<DateTime, double>> PerHour(long idSession)
        {
            var sql = @"
                select
                    hour_in_day as X,
	                exec_count  as Y
                from stat_usage_per_hour_in_day_v
                where id_session = @idSession;";
            return _db.Connection.Query<DataPoint<DateTime, double>>(sql, new { idSession });
        }

        public IEnumerable<DataPoint<DateTime, double>> PerMonth(long idSession)
        {
            var sql = @"
                select
                    month      as X,
	                exec_count as Y
                from stat_usage_per_month_v
                where id_session = @idSession;";
            return _db.Connection.Query<DataPoint<DateTime, double>>(sql, new { idSession });
        }

        #endregion Methods
    }
}