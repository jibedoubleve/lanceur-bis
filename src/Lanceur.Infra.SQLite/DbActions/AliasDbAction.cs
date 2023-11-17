﻿using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.Entities;
using Lanceur.Infra.SQLite.Helpers;
using Lanceur.SharedKernel.Mixins;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite.DbActions
{
    public class AliasDbAction
    {
        #region Fields

        private readonly IDbConnectionManager _db;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public AliasDbAction(IDbConnectionManager db, IAppLoggerFactory logFactory)
        {
            _db = db;
            _log = logFactory.GetLogger<AliasDbAction>();
        }

        #endregion Constructors

        #region Methods

        private void ClearAlias(params long[] ids)
        {
            const string sql = @"
                    delete from alias
                    where id = @id";

            var cnt = _db.DeleteMany(sql, ids);
            var idd = string.Join(", ", ids);
            _log.Info($"Removed '{cnt}' row(s) from alias. Id: {idd}");
        }

        private void ClearAliasArgument(params long[] ids)
        {
            const string sql = @"
                delete from alias_argument
                where id_alias = @id";

            var cnt = _db.DeleteMany(sql, ids);
            var idd = string.Join(", ", ids);
            _log.Debug($"Removed '{cnt}' row(s) from 'alias_argument'. Id: {idd}");
        }

        private void ClearAliasName(params long[] ids)
        {
            const string sql = @"
                delete from alias_name
                where id_alias = @id";

            var cnt = _db.DeleteMany(sql, ids);
            var idd = string.Join(", ", ids);
            _log.Debug($"Removed '{cnt}' row(s) from 'alias_name'. Id: {idd}");
        }

        private void ClearAliasUsage(params long[] ids)
        {
            const string sql = @"
                delete from alias_usage
                where id_alias = @id;";

            var cnt = _db.DeleteMany(sql, ids);
            var idd = string.Join(", ", ids);
            _log.Debug($"Removed '{cnt}' row(s) from 'alias_usage'. Id: {idd}");
        }

        private void CreateAdditionalParameters(AliasQueryResult alias, SQLiteTransaction tx)
            => CreateAdditionalParameters(alias.Id, alias.AdditionalParameters, tx);

        private void CreateAdditionalParameters(long idAlias, IEnumerable<QueryResultAdditionalParameters> parameters, SQLiteTransactionBase tx)
        {
            // Remove existing additional alias parameters
            const string sql1 = "delete from alias_argument where id_alias = @idAlias";
            tx.Connection.Execute(sql1, new { idAlias });

            // Create alias additional parameters
            const string sql2 = @"
                insert into alias_argument (id_alias, argument, name)
                values(@idAlias, @parameter, @name);";
            tx.Connection.Execute(sql2, parameters.ToEntity(idAlias));
        }

        private void UpdateName(AliasQueryResult alias, SQLiteTransactionBase tx)
        {
            //Remove all names
            const string sql = @"delete from alias_name where id_alias = @id";
            tx.Connection.Execute(sql, new { id = alias.Id });

            //Recreate new names
            const string sql2 = @"insert into alias_name (id_alias, name) values (@id, @name)";
            foreach (var name in alias.Synonyms.SplitCsv())
            {
                tx.Connection.Execute(sql2, new { id = alias.Id, name });
            }
        }

        internal IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> results)
        {
            var sql = @$"
                select
	                id_alias as {nameof(KeywordUsage.Id)},
	                count
                from
	                stat_usage_per_app_v
                where id_alias in @ids;";

            var dbResultAr = results as QueryResult[] ?? results.ToArray();
            var dbResults = _db.WithinTransaction(
                tx => tx.Connection.Query<KeywordUsage>(sql, new { ids = dbResultAr.Select(x => x.Id).ToArray() })
                            .ToArray()
            );

            foreach (var result in dbResultAr)
            {
                result.Count = (from item in dbResults
                                where item.Id == result.Id
                                select item.Count).SingleOrDefault();
            }
            return dbResultAr;
        }

        public bool CheckNameExists(AliasQueryResult alias, long idSession)
        {
            const string sql = @"
            select count(*)
            from
	            alias_name an
                inner join alias a on an.id_alias = a.id
            where
            	lower(name) = @name
                and a.id_session = @idSession";

            var count = _db.WithinTransaction(tx => tx.Connection.ExecuteScalar<int>(sql, new
            {
                name = alias.Name,
                idSession
            }));
            return count > 0;
        }

        public ExistingNameResponse CheckNameExists(string[] names, long idSession)
        {
            const string sql = @"
                select an.name
                from
                	alias_name an
                	inner join alias a on a.id = an.id_alias
                where
                    an.name in @names
                    and a.id_session = @idSession";

            var result = _db.WithinTransaction(
                tx => tx.Connection.Query<string>(sql, new { names, idSession })
                            .ToArray()
            );

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
                    thumbnail,
                    lua_script,
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
                    @thumbnail,
                    @luaScript,
                    @isHidden
                );
                select last_insert_rowid() from alias limit 1;";
            var param = new
            {
                Arguments = alias.Parameters,
                alias.FileName,
                alias.Notes,
                alias.RunAs,
                alias.StartMode,
                alias.WorkingDirectory,
                idSession,
                alias.Icon,
                alias.Thumbnail,
                alias.LuaScript,
                alias.IsHidden
            };

            var csv = alias.Synonyms.SplitCsv();
            var additionalParameters = alias.AdditionalParameters;
            var id = _db.WithinTransaction(tx =>
            {
                var id = tx.Connection.ExecuteScalar<long>(sqlAlias, param);

                // Create synonyms
                const string sqlSynonyms = @"insert into alias_name (id_alias, name) values (@id, @name)";
                foreach (var name in csv)
                {
                    tx.Connection.ExecuteScalar<long>(sqlSynonyms, new
                    {
                        id,
                        name
                    });
                }

                //Create additional arguments
                CreateAdditionalParameters(id, additionalParameters, tx);
                return id;
            });

            alias.Id = id;

            // Return either the id of the created Alias or
            // return the id of the just updated alias (which
            // should be the same as the one specified as arg)
            return id;
        }

        public void CreateInvisible(ref QueryResult alias)
        {
            var queryResult = new AliasQueryResult
            {
                Name = alias.Name,
                FileName = alias.Name,
                Synonyms = alias.Name,
                IsHidden = true,
                Icon = "PageHidden"
            };
            var idSession = GetDefaultSessionId();
            alias.Id = Create(ref queryResult, idSession);
        }

        public long GetDefaultSessionId()
        {
            var sql = "select s_value from settings where lower(s_key) = 'idsession'";
            var result = _db.WithinTransaction(tx => tx.Connection.Query<long>(sql).FirstOrDefault());
            return result;
        }

        /// <summary>
        /// Get the a first alias with the exact name. In case of multiple aliases
        /// with same name, the one with greater counter is selected.
        /// </summary>
        /// <param name="name">The alias' exact name to find.</param>
        /// <param name="idSession">The session where the search occurs.</param>
        /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
        /// <returns>The exact match or <c>null</c> if not found.</returns>
        public AliasQueryResult GetExact(string name, long? idSession = null, bool includeHidden = false)
        {
            idSession ??= GetDefaultSessionId();

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
                    a.thumbnail   as {nameof(AliasQueryResult.Thumbnail)},
                    a.lua_script  as {nameof(AliasQueryResult.LuaScript)},
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
                    n.name";

            var hidden = includeHidden ? new[] { 0, 1 } : 0.ToEnumerable();
            var arguments = new { idSession, name, hidden };

            return _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, arguments).FirstOrDefault());
        }

        public KeywordUsage GetHiddenKeyword(string name)
        {
            const string sql = @"
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
            var result = _db.WithinTransaction(tx => tx.Connection.Query<KeywordUsage>(sql, new { name }).FirstOrDefault());

            return result;
        }

        public void Remove(AliasQueryResult alias)
        {
            if (alias == null) { throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias."); }

            ClearAliasUsage(alias.Id);
            ClearAliasArgument(alias.Id);
            ClearAliasName(alias.Id);
            ClearAlias(alias.Id);
        }

        public void Remove(IEnumerable<SelectableAliasQueryResult> alias)
        {
            ArgumentNullException.ThrowIfNull(nameof(alias));
            var ids = alias.Select(x => x.Id).ToArray();

            ClearAliasUsage(ids);
            ClearAliasArgument(ids);
            ClearAliasName(ids);
            ClearAlias(ids);
        }

        public long Update(AliasQueryResult alias)
        {
            const string sql = @"
                update alias
                set
                    arguments   = @parameters,
                    file_name   = @fileName,
                    notes       = @notes,
                    run_as      = @runAs,
                    start_mode  = @startMode,
                    working_dir = @WorkingDirectory,
                    icon        = @Icon,
                    thumbnail   = @thumbnail,
                    lua_script  = @luaScript                                   
                where id = @id;";

            _db.WithinTransaction(tx =>
            {
                tx.Connection.Execute(sql, new
                {
                    alias.Parameters,
                    alias.FileName,
                    alias.Notes,
                    alias.RunAs,
                    alias.StartMode,
                    alias.WorkingDirectory,
                    alias.Icon,
                    alias.Thumbnail,
                    alias.LuaScript,
                    id = alias.Id
                });
                CreateAdditionalParameters(alias, tx);
                UpdateName(alias, tx);
            });

            alias.SynonymsWhenLoaded = alias.Synonyms;
            return alias.Id;
        }

        #endregion Methods
    }
}