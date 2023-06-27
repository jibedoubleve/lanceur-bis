using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

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

        private void UpdateName(AliasQueryResult alias)
        {
            //Remove all names
            var sql = @"delete from alias_name where id_alias = @id";
            _db.Connection.Execute(sql, new { id = alias.Id });

            //Recreate new names
            sql = @"insert into alias_name (id_alias, name) values (@id, @name)";
            foreach (var name in alias.Synonyms.SplitCsv())
            {
                _db.Connection.Execute(sql, new { id = alias.Id, name });
            }
        }

        internal IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> results)
        {
            var ids = (from r in results
                       where r.Id != 0
                       select r);
            var sql = @$"
                select
	                id_alias as {nameof(KeywordUsage.Id)},
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

        public ExistingNameResponse CheckNameExists(string[] names, long idSession)
        {
            var sql = @"
                select an.name
                from
                	alias_name an
                	inner join alias a on a.id = an.id_alias
                where
                    an.name in @names
                    and a.id_session = @idSession";

            var result = _db.Connection
                            .Query<string>(sql, new { names, idSession })
                            .ToArray();

            return new(result);
        }

        public long Create(ref AliasQueryResult alias, long idSession)
        {
            const string sqlAlias = @"
                insert into alias (
                    arguments,
                    file_name,
                    notes,
                    run_as,
                    start_mode,
                    working_dir,
                    id_session,
                    icon,
                    hidden
                ) values (
                    @arguments,
                    @fileName,
                    @notes,
                    @runAs,
                    @startMode,
                    @workingDirectory,
                    @idSession,
                    @icon,
                    @isHidden
                );
                select last_insert_rowid() from alias limit 1;";
            var id = _db.Connection.ExecuteScalar<long>(sqlAlias, new
            {
                Arguments = alias.Parameters,
                alias.FileName,
                alias.Notes,
                alias.RunAs,
                alias.StartMode,
                alias.WorkingDirectory,
                idSession,
                alias.Icon,
                alias.IsHidden
            });

            // Create synonyms
            const string sqlSynonyms = @"insert into alias_name (id_alias, name) values (@id, @name)";
            foreach (var name in alias.Synonyms.SplitCsv())
            {
                _db.Connection.ExecuteScalar<long>(sqlSynonyms, new
                {
                    id,
                    name
                });
            }

            alias.Id = id;

            // Return either the id of the created Alias or
            // return the id of the just updated allias (which
            // should be the same as the one specified as arg
            return id;
        }

        public void CreateInvisible(ref QueryResult alias)
        {
            var aliasQR = new AliasQueryResult
            {
                Name = alias.Name,
                FileName = alias.Name,
                Synonyms = alias.Name,
                IsHidden = true,
                Icon = "PageHidden"
            };
            var idSession = GetDefaultSessionId();
            alias.Id = Create(ref aliasQR, idSession);
        }

        public void Dispose() => _db.Dispose();

        public long GetDefaultSessionId()
        {
            var sql = "select s_value from settings where lower(s_key) = 'idsession'";
            var result = _db.Connection.Query<long>(sql).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Get the a first alias with the exact name. In case of multiple aliases
        /// with same name, the one with greater counter is selected.
        /// </summary>
        /// <param name="name">The alias' exact name to find.</param>
        /// <param name="idSession">The session where the search occurs.</param>
        /// <param name="delay">Indicates a delay to set.</param>
        /// <returns>The exact match or <c>null</c> if not found.</returns>
        public AliasQueryResult GetExact(string name, long? idSession = null, bool includeHidden = false)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            var sql = @$"
                select
                    n.Name        as {nameof(AliasQueryResult.Name)},
                    s.synonyms    as {nameof(AliasQueryResult.Synonyms)},
                    a.Id          as {nameof(AliasQueryResult.Id)},
                    a.arguments   as {nameof(AliasQueryResult.Parameters)},
                    a.file_name   as {nameof(AliasQueryResult.FileName)},
                    a.notes       as {nameof(AliasQueryResult.Notes)},
                    a.run_as      as {nameof(AliasQueryResult.RunAs)},
                    a.start_mode  as {nameof(AliasQueryResult.StartMode)},
                    a.working_dir as {nameof(AliasQueryResult.WorkingDirectory)},
                    a.icon        as {nameof(AliasQueryResult.Icon)},
                    c.exec_count  as {nameof(AliasQueryResult.Count)},
                    a.hidden      as {nameof(AliasQueryResult.IsHidden)}
                from
                    alias a
                    left join alias_name n on a.id = n.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias = a.id
                where
                    a.id_session = @idSession
                    and n.Name = @name
                    and hidden in @hidden
                order by
                    c.exec_count desc,
                    n.name asc";

            var hidden = includeHidden ? new int[] { 0, 1 } : 0.ToEnumerable();
            var arguments = new { idSession, name, hidden };

            return _db.Connection.Query<AliasQueryResult>(sql, arguments).FirstOrDefault();
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

        public void Remove(AliasQueryResult alias)
        {
            if (alias == null) { throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias."); }

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
            alias.SynomymsPreviousState = alias.Synonyms;
            return alias.Id;
        }

        #endregion Methods
    }
}