using Dapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;

namespace Lanceur.Infra.SQLite.DbActions
{
    public class AliasDbAction : IDisposable
    {
        #region Fields

        private readonly SQLiteConnectionScope _db;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public AliasDbAction(SQLiteConnectionScope db, IAppLoggerFactory logFactory)
        {
            _db = db;
            _log = logFactory.GetLogger<AliasDbAction>();
        }

        #endregion Constructors

        #region Methods

        public bool CheckNameExists(AliasQueryResult alias, long idSession)
        {
            var sql = @"
            select count(*)
            from
	            alias_name an
                inner join alias a on an.id_alias = a.id
            where
            	lower(name) = @name
                and a.id_session = @idSession";

            var count = _db.Connection.ExecuteScalar<int>(sql, new
            {
                name = alias.Name,
                idSession
            });
            return count > 0;
        }

        public long Create(ref AliasQueryResult alias, long idSession)
        {
            //Create the alias
            var sql = @"
                insert into alias (
                    arguments,
                    file_name,
                    notes,
                    run_as,
                    start_mode,
                    id_session,
                    icon,
                    working_dir
                ) values (
                    @arguments,
                    @fileName,
                    @notes,
                    @runAs,
                    @startMode,
                    @idSession,
                    @icon,
                    @workingDirectory
                );
                select last_insert_rowid() from alias limit 1;";

            var id = _db.Connection.ExecuteScalar<long>(sql, new
            {
                Arguments = alias.Parameters,
                alias.FileName,
                alias.Notes,
                alias.RunAs,
                alias.StartMode,
                idSession,
                alias.Icon,
                alias.WorkingDirectory
            });

            alias.Id = id;

            Duplicate(alias);

            // Return either the id of the created Alias or
            // return the id of the just updated allias (which
            // should be the same as the one specified as arg
            return id;
        }

        public void Dispose() => _db.Dispose();

        public void Duplicate(AliasQueryResult alias)
        {
            //Create the alias name
            var sql2 = @"insert into alias_name (id_alias, name) values (@idAlias, @name);";
            _db.Connection.Execute(sql2, new
            {
                idAlias = alias.DuplicateOf ?? alias.Id,
                alias.Name,
            });

            if (alias.Id == 0)
            {
                alias.DuplicateOf = null; //It's not a duplicate anymore, we've created it in the DB
            }
        }

        public long GetDefaultSessionId()
        {
            var sql = "select s_value from settings where lower(s_key) = 'idsession'";
            var result = _db.Connection.Query<long>(sql).FirstOrDefault();
            return result;
        }

        public AliasQueryResult GetExact(string name, long? idSession = null, int delay = 0)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            var sql = @"
                select
                    n.Name        as Name,
                    n.Name        as OldName,
                    s.Id          as Id,
                    s.id          as Id,
                    s.arguments   as Arguments,
                    s.file_name   as FileName,
                    s.notes       as Notes,
                    s.run_as      as RunAs,
                    s.start_mode  as StartMode,
                    s.working_dir as WorkingDirectory,
                    s.icon        as Icon,
                    c.exec_count  as Count
                from
                    alias s
                    left join alias_name n on s.id = n.id_alias
                    left join stat_execution_count_v c on c.id_keyword = s.id
                where
                    s.id_session = @idSession
                    and n.Name = @name
                order by
                    c.exec_count desc,
                    n.name asc";
            var arguments = new { idSession, name };

            var result = _db.Connection.Query<AliasQueryResult>(sql, arguments).FirstOrDefault();

            if (result is not null) { result.Delay = delay; }

            return result;
        }

        public void Remove(AliasQueryResult alias)
        {
            if (alias == null) { throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias."); }
            else if (alias.Name == null) { throw new ArgumentNullException(nameof(alias.Name), "Cannot delete alias with no name."); }
            else
            {
                var sql1 = @"delete from alias_usage where id_alias = @id_alias";
                var cnt = _db.Connection.Execute(sql1, new { id_alias = alias.Id });
                _log.Debug($"Removed '{cnt}' row(s) from alias_usage. Id: {alias.Id}");

                var sql2 = @"delete from alias_name where id_alias = @id_alias";
                cnt = _db.Connection.Execute(sql2, new { id_alias = alias.Id });
                _log.Debug($"Removed '{cnt}' row(s) from alias_name. Id: {alias.Id}");

                // If alias refers to no other names, remove it.
                // This query, cleans all alias in this situation.
                var sql3 = @"
                    delete from alias
                    where id in (
                      select a.id
                      from 
                          alias a
                          left join alias_name an on a.id = an.id_alias
                      where an.id_alias is null
                    );";
                _db.Connection.Execute(sql3);
                _log.Debug($"Removed '{cnt}' row(s) from alias. Id: {alias.Id}");
            }
        }

        public void Remove(IEnumerable<SelectableAliasQueryResult> alias)
        {
            if (alias == null) { throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias."); }
            else
            {
                var sql1 = @"delete from alias_usage where id_alias in @id_alias";
                _db.Connection.Execute(sql1, new { id_alias = alias.Select(x => x.Id).ToArray() });

                var sql2 = @"delete from alias_name where id_alias in @id_alias";
                _db.Connection.Execute(sql2, new { id_alias = alias.Select(x => x.Id).ToArray() });

                // If alias refers to no other names, remove it.
                // This query, cleans all alias in this situation.
                var sql3 = @"
                    delete from alias
                    where id in (
                      select a.id
                      from 
                          alias a
                          left join alias_name an on a.id = an.id_alias
                      where an.id_alias is null
                    );";
                _db.Connection.Execute(sql3);
            }
        }

        public void SetUsage(QueryResult alias)
        {
            if ((alias?.Id ?? 0) == 0) { _log.Trace($"Try to set usage to unsupported Alias with this name'{(alias?.Name ?? "N.A.")}'"); }
            else
            {
                var idSession = GetDefaultSessionId();
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
            }
        }

        public long Update(AliasQueryResult alias)
        {
            var sql = @"
                update alias
                set
                    arguments   = @parameters,
                    file_name   = @fileName,
                    notes       = @notes,
                    run_as      = @runAs,
                    start_mode  = @startMode,
                    working_dir = @WorkingDirectory,
                    icon        = @Icon
                where id = @id;";

            _db.Connection.Execute(sql, new
            {
                alias.Parameters,
                alias.FileName,
                alias.Notes,
                alias.RunAs,
                alias.StartMode,
                alias.WorkingDirectory,
                alias.Icon,
                id = alias.Id
            });

            UpdateName(alias);
            return alias.Id;
        }

        public void UpdateName(AliasQueryResult alias)
        {
            if (alias.HasChangedName())
            {
                var sql = @"
                select id
                from alias_name
                where
	                id_alias = @id
                    and lower(name) = lower(@name)";
                var id = _db.Connection.Query<long?>(sql, new
                {
                    id = alias.Id,
                    name = alias.OldName
                }).FirstOrDefault();

                if (id.HasValue)
                {
                    sql = @"
                    update alias_name
                    set
                        name = lower(@name)
                    where
                        id = @id;";

                    _db.Connection.Execute(sql, new
                    {
                        id,
                        name = alias.Name
                    });
                }
            }
        }

        public KeywordUsage GetHiddenKeyword(string name)
        {
            var sql = @"
                select 
	                a.id,
                    n.name,
                    (
    	                select count(id)
     	                from alias_usage
      	                where id = a.id
                    ) as count
                from
	                alias a
                    inner join alias_name n on a.id = n.id_alias
                where 
	                hidden = 1
                    and n.name = @name;";
            var result = _db.Connection.Query<KeywordUsage>(sql, new { name }).FirstOrDefault();

            return result;
        }

        internal IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> results)
        {
            var ids = (from r in results
                       where r.Id != 0
                       select r);
            var sql = @"
                select
	                id_alias as id,
	                count
                from 
	                stat_usage_per_app_v
                where id_alias in @ids;";

            var dbResults = _db.Connection.Query<KeywordUsage>(sql, new { ids = results.Select(x => x.Id).ToArray() });

            foreach (var result in results)
            {
                result.Count = (from item in dbResults
                                where item.Id == result.Id
                                select item.Count).SingleOrDefault();
            }
            return results;
        }

        #endregion Methods
    }
}