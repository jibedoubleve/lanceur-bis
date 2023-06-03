using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using System.Text.RegularExpressions;

namespace Lanceur.Infra.SQLite
{
    public partial class SQLiteRepository : SQLiteRepositoryBase, IDbRepository
    {
        #region Fields

        private readonly AliasDbAction _aliasDbAction;
        private readonly IConvertionService _converter;
        private readonly IAppLogger _log;
        private readonly MacroDbAction _macroManager;

        #endregion Fields

        #region Constructors

        public SQLiteRepository(
            SQLiteConnectionScope scope,
            IAppLoggerFactory logFactory,
            IConvertionService converter) : base(scope)
        {
            _log = logFactory.GetLogger<SQLiteRepository>();
            _converter = converter;
            _aliasDbAction = new AliasDbAction(scope, logFactory);
            _macroManager = new MacroDbAction(DB, logFactory, converter);
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<AliasQueryResult> GetAll(long? idSession = null)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            var sql = @"
                select
                    n.Name        as Name,
                    n.Name        as OldName,
                    s.Id          as Id,
                    s.id          as Id,
                    s.arguments   as Parameters,
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
                order by
                    c.exec_count desc,
                    n.name asc";
            var parameters = new { idSession };

            var result = DB.Connection.Query<AliasQueryResult>(sql, parameters);
            return result ?? AliasQueryResult.NoResult;
        }

        public Session GetDefaultSession()
        {
            var id = GetDefaultSessionId();
            var sql = @"
                select
	                id    as Id,
	                name  as Name,
                    notes as Notes
                from
	                alias_session
                where
	                id = @id";
            var result = DB.Connection.Query<Session>(sql, new { id }).SingleOrDefault();
            return result;
        }

        public long GetDefaultSessionId() => _aliasDbAction.GetDefaultSessionId();

        public IEnumerable<QueryResult> GetDoubloons()
        {
            var sql = @"
            select
                id        as Id,
                file_name as Description,
                file_name as FileName,
                name      as Name
            from
                data_doubloons_v
            order by file_name";
            var results = DB.Connection.Query<SelectableAliasQueryResult>(sql);
            return results;
        }

        public AliasQueryResult GetExact(string name, long? idSession = null) => _aliasDbAction.GetExact(name, idSession);

        public IEnumerable<SelectableAliasQueryResult> GetInvalidAliases()
        {
            var macro = new Regex("^@.*@$");
            var abs = new Regex(@"[a-zA-Z]:\\");

            var result = (from a in GetAll()
                          where Uri.TryCreate(a.Description, UriKind.RelativeOrAbsolute, out _) == true
                             && File.Exists(a.Description) == false
                             && macro.IsMatch(a.Description) == false
                             && abs.IsMatch(a.Description)
                          select a);

            return _converter.ToSelectableQueryResult(result);
        }

        public KeywordUsage GetKeyword(string name) => _aliasDbAction.GetHiddenKeyword(name);

        public IEnumerable<QueryResult> GetMostUsedAliases(long? idSession = null)
        {
            idSession ??= GetDefaultSessionId();
            var sql = @"
                select
	                keywords   as name,
                    exec_count as count
                from
                    stat_execution_count_v
                where
                    id_session = @idSession
                order
                    by exec_count desc";
            return DB.Connection.Query<DisplayUsageQueryResult>(sql, new { idSession });
        }

        public IEnumerable<Session> GetSessions()
        {
            var sql = @"
            select
	            id    as Id,
                name  as Name,
                notes as Notes
            from alias_session";
            var result = DB.Connection.Query<Session>(sql);
            return result;
        }

        public IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per, long? idSession = null)
        {
            idSession ??= GetDefaultSessionId();
            var action = new HistoryDbAction(DB);
            return per switch
            {
                Per.Hour => action.PerHour(idSession.Value),
                Per.Day => action.PerDay(idSession.Value),
                Per.DayOfWeek => action.PerDayOfWeek(idSession.Value),
                Per.Month => action.PerMonth(idSession.Value),
                _ => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level"),
            };
        }

        public void HydrateMacro(QueryResult alias)
        {
            var sql = @"
            select
	            a.id        as Id,
                count(a.id) as Count
            from
	            alias a
                inner join alias_usage au on a.id = au.id_alias
            where
	            file_name like @name
            group by
	            a.id;";

            var results = DB.Connection.Query<dynamic>(sql, new { name = alias.Name });

            if (results.Count() == 1)
            {
                var item = results.ElementAt(0);
                alias.Id = item.Id;
                alias.Count = (int)item.Count;
            }
        }

        public IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result) => _aliasDbAction.RefreshUsage(result);

        public void Remove(AliasQueryResult alias) => _aliasDbAction.Remove(alias);

        public void Remove(IEnumerable<SelectableAliasQueryResult> doubloons)
        {
            _log.Info($"Removing {doubloons.Count()} alias(es)");
            _aliasDbAction.Remove(doubloons);
        }

        public void Remove(Session session)
        {
            _log.Info($"Removes session with name '{session.Name}'");
            var action = new SessionDbAction(DB);
            action.Remove(session);
        }

        /// <summary>
        ///     Create a new alias if its id is '0' to the specified session (if not specified, to the default session)
        ///     If the id exists, it'll update with the new information
        /// </summary>
        /// <param name="alias">The alias to create or update</param>
        /// <param name="idSession">
        ///     If the alias has to be created, it'll be linked to this session. This argument is ignored if the alias
        ///     needs to be updated. If not specified, the default session is selected.
        /// </param>
        /// <returns>
        ///     The id of the updated/created alias
        /// </returns>
        public void SaveOrUpdate(ref AliasQueryResult alias, long? idSession = null)
        {
            if (alias == null) { throw new ArgumentNullException(nameof(alias), $"Cannot save NULL alias."); }
            if (alias.Name == null) { throw new ArgumentNullException(nameof(alias.Name), "Cannot create or update alias with no name."); }
            if (alias.Id < 0) { throw new NotSupportedException($"The alias has an unexpected id. (Name: {alias.Name})"); }

            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            if (alias.DuplicateOf.HasValue)
            {
                _log.Info($"Saving a duplicated alias. (Duplicate of: {alias.DuplicateOf})");
                _aliasDbAction.Duplicate(alias);
            }
            else if (alias.Id == 0 && !_aliasDbAction.CheckNameExists(alias, idSession.Value))
            {
                _aliasDbAction.Create(ref alias, idSession.Value);
                _log.Info($"Just created new alias. (Id: {alias.Id})");
            }
            else if (alias.Id > 0)
            {
                _log.Info($"Updating alias. (Id: {alias.Id})");
                _aliasDbAction.Update(alias);
            }
        }

        public IEnumerable<AliasQueryResult> Search(string name, long? idSession = null)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            var sql = @"
                select
                    n.Name        as Name,
                    n.Name        as OldName,
                    s.Id          as Id,
                    s.id          as Id,
                    s.arguments   as Parameters,
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
                    and n.Name like @name
                    and s.hidden = 0
                order by
                    c.exec_count desc,
                    n.name asc";

            name = $"{name ?? string.Empty}%";
            var results = DB.Connection.Query<AliasQueryResult>(sql, new { name, idSession });

            results = _macroManager.UpgradeToComposite(results);
            return results ?? AliasQueryResult.NoResult;
        }

        public void SetDefaultSession(long idSession)
        {
            var sql = @"
            update settings
            set
                s_value = @idSession
            where
                lower(s_key) = 'idsession'";
            DB.Connection.Execute(sql, new { idSession });
        }

        public void SetUsage(QueryResult alias) => _aliasDbAction.SetUsage(alias);

        public void Update(ref Session session)
        {
            var action = new SessionDbAction(DB);

            if (session == null) { throw new ArgumentNullException(nameof(session)); }
            else if (session.Id == 0) { action.Create(ref session); }
            else { action.Update(session); }
        }

        #endregion Methods
    }
}