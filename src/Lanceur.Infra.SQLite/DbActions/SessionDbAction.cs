using Dapper;
using Lanceur.Core.Models;

namespace Lanceur.Infra.SQLite.DbActions
{
    internal class SessionDbAction : IDisposable
    {
        #region Fields

        private readonly SQLiteConnectionScope _db;

        #endregion Fields

        #region Constructors

        public SessionDbAction(SQLiteConnectionScope db)
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
            var id = _db.Connection.ExecuteScalar<long>(sql, new { session.Name, session.Notes });
            session.Id = id;
        }

        public void Dispose() => _db.Dispose();

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

            foreach (var sql in queries)
            {
                _db.Connection.Execute(sql, new { session.Id });
            }
        }

        public void Update(Session session)
        {
            var sql = @"
                update alias_session
                set
                    name  = @name,
                    notes = @notes
                where
                    id = @id;";
            _db.Connection.Execute(sql, new { session.Name, session.Notes, session.Id });
        }

        #endregion Methods
    }
}