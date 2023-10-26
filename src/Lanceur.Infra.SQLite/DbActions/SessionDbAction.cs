using Dapper;
using Lanceur.Core.Models;

namespace Lanceur.Infra.SQLite.DbActions
{
    internal class SessionDbAction
    {
        #region Fields

        private readonly ISQLiteConnectionScope _db;

        #endregion Fields

        #region Constructors

        public SessionDbAction(ISQLiteConnectionScope db)
        {
            _db = db;
        }

        #endregion Constructors

        #region Methods

        public void Create(ref Session session)
        {
            var sql = @"
            insert into alias_session (name, notes) values (@name, @notes);
            select last_insert_rowid() limit 1;";
            var param = new { session.Name, session.Notes };
            var id = _db.WithinTransaction(tx => tx.Connection.ExecuteScalar<long>(sql, param));
            session.Id = id;
        }

        public void Remove(Session session)
        {
            var queries = new List<string>
            {
                "delete from alias_usage where id_session = @id",
                @"
                delete
                from
	                alias_name
                where id_alias in (
	                select id from alias where id_session = @id
                );",
                "delete from alias where id_session = @id",
                "delete from alias_session where id = @id",
            };

            _db.WithinTransaction(tx =>
            {
                foreach (var sql in queries)
                {
                    tx.Connection.Execute(sql, new { session.Id });
                }
            });
        }

        public void Update(Session session)
        {
            const string sql = @"
                update alias_session
                set
                    name  = @name,
                    notes = @notes
                where
                    id = @id;";

            _db.WithinTransaction(tx => tx.Connection.Execute(sql, new { session.Name, session.Notes, session.Id }));
        }

        #endregion Methods
    }
}