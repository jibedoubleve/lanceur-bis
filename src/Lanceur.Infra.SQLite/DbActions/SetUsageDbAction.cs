using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.SQLite.DbActions
{
    public class SetUsageDbAction : IDisposable
    {
        #region Fields

        private readonly AliasDbAction _aliasDbAction;
        private readonly SQLiteConnectionScope _db;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public SetUsageDbAction(SQLiteConnectionScope db, IAppLoggerFactory logFactory)
        {
            _db = db;
            _log = logFactory.GetLogger<AliasDbAction>();

            _aliasDbAction = new AliasDbAction(db, logFactory);
        }

        #endregion Constructors

        #region Methods

        public void Dispose() => _db.Dispose();

        public void SetUsage(ref QueryResult alias, long idSession)
        {
            if ((alias?.Id ?? 0) == 0)
            {
                if (_aliasDbAction.GetExact(alias.Name) is { } a)
                {
                    alias.Id = a.Id;
                }
                else { _aliasDbAction.CreateInvisible(ref alias); }
            }

            var sql = @"
                    insert into alias_usage (
                        id_alias,
                        id_session,
                        time_stamp

                    ) values (
                        @idAlias,
                        @idSession,
                        @now
                    )";
            _db.Connection.Execute(sql, new { idAlias = alias.Id, idSession, now = DateTime.Now });
            alias.Count++;
        }

        #endregion Methods
    }
}