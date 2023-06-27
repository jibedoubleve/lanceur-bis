using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Mixins;
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
        private readonly SetUsageDbAction _setUsageDbAction;

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
            _setUsageDbAction = new SetUsageDbAction(DB, logFactory);
        }

        #endregion Constructors

        #region Methods

        public ExistingNameResponse CheckNamesExist(string[] names, long? idSession = null)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }
            return _aliasDbAction.CheckNameExists(names, idSession.Value);
        }

        public IEnumerable<AliasQueryResult> GetAll(long? idSession = null)
        {
            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            var sql = @"
                select
                    an.Name       as Name,
                    a.Id          as Id,
                    a.id          as Id,
                    a.arguments   as Parameters,
                    a.file_name   as FileName,
                    a.notes       as Notes,
                    a.run_as      as RunAs,
                    a.start_mode  as StartMode,
                    a.working_dir as WorkingDirectory,
                    a.icon        as Icon,
                    c.exec_count  as Count,
                    s.synonyms    as Synonyms,
                    s.Synonyms    as SynomymsPrevious
                from
                    alias a
                    left join alias_name an on a.id = an.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias = a.id
                where
                    a.id_session = @idSession
                    and a.hidden = 0
                order by
                    c.exec_count desc,
                    an.name asc";
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

        /// <summary>
        /// Get the a first alias with the exact name. In case of multiple aliases
        /// with same name, the one with greater counter is selected.
        /// </summary>
        /// <param name="name">The alias' exact name to find.</param>
        /// <param name="idSession">The session where the search occurs.</param>
        /// <param name="delay">Indicates a delay to set.</param>
        /// <returns>The exact match or <c>null</c> if not found.</returns>
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
            if (alias.Synonyms == null) { throw new ArgumentNullException(nameof(alias.Name), "Cannot create or update alias with no name."); }
            if (alias.Id < 0) { throw new NotSupportedException($"The alias has an unexpected id. (Name: {alias.Name})"); }

            if (!idSession.HasValue) { idSession = GetDefaultSessionId(); }

            if (alias.Id == 0 && !_aliasDbAction.CheckNameExists(alias, idSession.Value))
            {
                _aliasDbAction.Create(ref alias, idSession.Value);
                _log.Info($"Created new alias. (Id: {alias.Id})");
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
                    an.Name       as Name,
                    a.Id          as Id,
                    a.id          as Id,
                    a.arguments   as Parameters,
                    a.file_name   as FileName,
                    a.notes       as Notes,
                    a.run_as      as RunAs,
                    a.start_mode  as StartMode,
                    a.working_dir as WorkingDirectory,
                    a.icon        as Icon,
                    c.exec_count  as Count,
                    s.synonyms    as Synonyms,
                    s.Synonyms    as SynomymsPrevious
                from
                    alias a
                    left join alias_name an on a.id = an.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias = a.id
                where
                    a.id_session = @idSession
                    and an.Name like @name
                    and a.hidden = 0
                order by
                    c.exec_count desc,
                    an.name asc";

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

        public void SetUsage(QueryResult alias)
        {
            if (alias is null)
            {
                _log.Info("Impossible to set usage: alias is null.;");
                return;
            }
            if (alias.Name.IsNullOrEmpty())
            {
                _log.Info("Impossible to set usage: alias name is empty.");
                return;
            }

            var idSession = GetDefaultSessionId();
            _setUsageDbAction.SetUsage(ref alias, idSession);
        }

        public void SetUsage(string aliasName) => SetUsage(new AliasQueryResult() { Name = aliasName });

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