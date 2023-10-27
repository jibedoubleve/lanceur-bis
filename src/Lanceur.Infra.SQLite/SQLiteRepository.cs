using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Mixins;
using System.Text.RegularExpressions;

namespace Lanceur.Infra.SQLite;

public class SQLiteRepository : SQLiteRepositoryBase, IDbRepository
{
    #region Fields

    private readonly AliasDbAction _aliasDbAction;
    private readonly AliasSearchDbAction _aliasSearchDbAction;
    private readonly IConvertionService _converter;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly IAppLogger _log;
    private readonly SetUsageDbAction _setUsageDbAction;

    #endregion Fields

    #region Constructors

    public SQLiteRepository(
        IDbConnectionManager manager,
        IAppLoggerFactory logFactory,
        IConvertionService converter) : base(manager)
    {
        ArgumentNullException.ThrowIfNull(logFactory);
        ArgumentNullException.ThrowIfNull(converter);

        _log = logFactory.GetLogger<SQLiteRepository>();
        _converter = converter;
        _aliasDbAction = new(manager, logFactory);
        _getAllAliasDbAction = new(manager, logFactory);
        _setUsageDbAction = new(DB, logFactory);
        _aliasSearchDbAction = new(DB, logFactory, converter);
    }

    #endregion Constructors

    #region Methods

    ///<inheritdoc/>
    public ExistingNameResponse CheckNamesExist(string[] names, long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        return _aliasDbAction.CheckNameExists(names, idSession.Value);
    }

    /// <summary>
    /// Returns all the alias of the specified session from the repository.
    /// If no session is specified, it returns all the alias.
    /// </summary>
    /// <param name="idSession">
    /// The alias should be link the the specified session.
    /// If no session specified, all the alias are returned
    /// </param>
    /// <returns>The alias of the specified session or all if no session
    /// specified</returns>
    public IEnumerable<AliasQueryResult> GetAll(long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        return _getAllAliasDbAction.GetAll(idSession.Value);
    }

    ///<inheritdoc/>
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters(long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        return _getAllAliasDbAction.GetAllAliasWithAdditionalParameters(idSession.Value);
    }

    ///<inheritdoc/>
    public Session GetDefaultSession()
    {
        var id = GetDefaultSessionId();
        const string sql = @$"
                select
	                id    as {nameof(Session.Id)},
	                name  as {nameof(Session.Name)},
                    notes as {nameof(Session.Notes)}
                from
	                alias_session
                where
	                id = @id";
        var result = DB.WithinTransaction(tx => tx.Connection.Query<Session>(sql, new { id }).SingleOrDefault());
        return result;
    }

    ///<inheritdoc/>
    public long GetDefaultSessionId() => _aliasDbAction.GetDefaultSessionId();

    ///<inheritdoc/>
    public IEnumerable<QueryResult> GetDoubloons()
    {
        const string sql = @$"
            select
                id        as {nameof(SelectableAliasQueryResult.Id)},
                file_name as {nameof(SelectableAliasQueryResult.Description)},
                file_name as {nameof(SelectableAliasQueryResult.FileName)},
                name      as {nameof(SelectableAliasQueryResult.Name)}
            from
                data_doubloons_v
            order by file_name";
        var results = DB.WithinTransaction(tx => tx.Connection.Query<SelectableAliasQueryResult>(sql));
        return results;
    }

    /// <summary>
    /// Get the a first alias with the exact name. In case of multiple aliases
    /// with same name, the one with greater counter is selected.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <param name="idSession">The session where the search occurs.</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    public AliasQueryResult GetExact(string name, long? idSession = null) => _aliasDbAction.GetExact(name, idSession);

    public IEnumerable<SelectableAliasQueryResult> GetInvalidAliases()
    {
        var macro = new Regex("^@.*@$");
        var abs = new Regex(@"[a-zA-Z]:\\");

        var result = from a in GetAll()
                     where a.Description != null
                           && Uri.TryCreate(a.Description, UriKind.RelativeOrAbsolute, out _)
                           && File.Exists(a.Description) == false
                           && macro.IsMatch(a.Description) == false
                           && abs.IsMatch(a.Description)
                     select a;

        return _converter.ToSelectableQueryResult(result);
    }

    ///<inheritdoc/>
    public KeywordUsage GetKeyword(string name) => _aliasDbAction.GetHiddenKeyword(name);

    ///<inheritdoc/>
    public IEnumerable<QueryResult> GetMostUsedAliases(long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        const string sql = @$"
                select
	                keywords   as {nameof(DisplayUsageQueryResult.Name)},
                    exec_count as {nameof(DisplayUsageQueryResult.Count)}
                from
                    stat_execution_count_v
                where
                    id_session = @idSession
                order
                    by exec_count desc";
        return DB.WithinTransaction(tx => tx.Connection.Query<DisplayUsageQueryResult>(sql, new { idSession }));
    }

    ///<inheritdoc/>
    public IEnumerable<Session> GetSessions()
    {
        const string sql = @$"
            select
	            id    as {nameof(Session.Id)},
                name  as {nameof(Session.Name)},
                notes as {nameof(Session.Notes)}
            from alias_session";
        var result = DB.WithinTransaction(tx => tx.Connection.Query<Session>(sql));
        return result;
    }

    ///<inheritdoc/>
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
            _ => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level")
        };
    }

    ///<inheritdoc/>
    public void Hydrate(QueryResult queryResult)
    {
        const string sql = @"
                select
	                a.id        as id,
                    count(a.id) as count
                from
	                alias a
                    inner join alias_name  an on a.id = an.id_alias
                    inner join alias_usage au on a.id = au.id_alias
                where an.name = @name
                group by a.id";

        var result = DB.WithinTransaction(
            tx => tx.Connection
                    .Query<AliasQueryResult>(sql, new { queryResult.Name })
                    .ToArray()
        );

        if (!result.Any()) return;

        var first = result.First();
        queryResult.Id = first.Id;
        queryResult.Count = first.Count;
    }

    /// <inheritdoc/>
    public void HydrateAlias(AliasQueryResult alias)
    {
        if (alias is null) return;

        const string sql = @"
            select
                id       as id,
                name     as name,
                argument as parameter
            from alias_argument
            where id_alias = @idAlias";
        var parameters =
            DB.WithinTransaction(tx => tx.Connection.Query<QueryResultAdditionalParameters>(sql, new { idAlias = alias.Id }));
        alias.AdditionalParameters = new(parameters);
    }

    ///<inheritdoc/>
    public void HydrateMacro(QueryResult alias)
    {
        const string sql = @$"
            select
	            a.id        as {nameof(QueryResult.Id)},
                count(a.id) as {nameof(QueryResult.Count)}
            from
	            alias a
                inner join alias_usage au on a.id = au.id_alias
            where
	            file_name like @name
            group by
	            a.id;";

        DB.WithinTransaction(tx =>
        {
            var results = tx.Connection.Query<dynamic>(sql, new { name = alias.Name });

            var enumerable = results as dynamic[] ?? results.ToArray();
            if (enumerable.Length != 1) return;

            var item = enumerable.ElementAt(0);
            alias.Id = item.Id;
            alias.Count = (int)item.Count;
        });
    }

    ///<inheritdoc/>
    public IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result) =>
        _aliasDbAction.RefreshUsage(result);

    ///<inheritdoc/>
    public void Remove(AliasQueryResult alias) => _aliasDbAction.Remove(alias);

    ///<inheritdoc/>
    public void Remove(IEnumerable<SelectableAliasQueryResult> doubloons)
    {
        var selectableAliasQueryResults = doubloons as SelectableAliasQueryResult[] ?? doubloons.ToArray();
        _log.Info($"Removing {selectableAliasQueryResults.Length} alias(es)");
        _aliasDbAction.Remove(selectableAliasQueryResults);
    }

    ///<inheritdoc/>
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
        ArgumentNullException.ThrowIfNull(alias, nameof(alias));
        ArgumentNullException.ThrowIfNull(alias.Synonyms, nameof(alias.Synonyms));
        ArgumentNullException.ThrowIfNull(alias.Id, nameof(alias.Id));

        idSession ??= GetDefaultSessionId();

        switch (alias.Id)
        {
            case 0 when !_aliasDbAction.CheckNameExists(alias, idSession.Value):
                _aliasDbAction.Create(ref alias, idSession.Value);
                _log.Info($"Created new alias. (Id: {alias.Id})");
                break;

            case > 0:
                _log.Info($"Updating alias. (Id: {alias.Id})");
                _aliasDbAction.Update(alias);
                break;
        }
    }

    ///<inheritdoc/>
    public IEnumerable<AliasQueryResult> Search(string name, long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        return _aliasSearchDbAction.Search(name, idSession.Value);
    }

    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria, long? idSession = null)
    {
        idSession ??= GetDefaultSessionId();
        return _aliasSearchDbAction.SearchAliasWithAdditionalParameters(criteria, idSession.Value);
    }

    public void SetDefaultSession(long idSession)
    {
        var sql = @"
            update settings
            set
                s_value = @idSession
            where
                lower(s_key) = 'idsession'";
        DB.WithinTransaction(tx => tx.Connection.Execute(sql, new { idSession }));
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

    ///<inheritdoc/>
    public void Update(ref Session session)
    {
        var action = new SessionDbAction(DB);

        if (session == null) throw new ArgumentNullException(nameof(session));
        else if (session.Id == 0) action.Create(ref session);
        else action.Update(session);
    }

    #endregion Methods
}