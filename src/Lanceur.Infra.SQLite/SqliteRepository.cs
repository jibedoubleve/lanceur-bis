using System.Reflection;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using Lanceur.Core.BusinessLogic;
using Lanceur.Infra.Macros;
using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur.Infra.SQLite;

public class SQLiteRepository : SQLiteRepositoryBase, IDbRepository
{
    #region Fields

    private readonly AliasDbAction _aliasDbAction;
    private readonly AliasSearchDbAction _aliasSearchDbAction;
    private readonly IMappingService _converter;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteRepository> _logger;
    private readonly SetUsageDbAction _setUsageDbAction;
    private static readonly MacroValidator MacroValidator = new(Assembly.GetAssembly(typeof(GuidMacro)));

    #endregion

    #region Constructors

    public SQLiteRepository(
        IDbConnectionManager manager,
        ILoggerFactory logFactory,
        IMappingService converter
    ) : base(manager)
    {
        ArgumentNullException.ThrowIfNull(logFactory);
        ArgumentNullException.ThrowIfNull(converter);

        _logger = logFactory.GetLogger<SQLiteRepository>();
        _converter = converter;
        _aliasDbAction = new(manager, logFactory);
        _getAllAliasDbAction = new(manager);
        _setUsageDbAction = new(DB, logFactory);
        _aliasSearchDbAction = new(DB, logFactory, converter);
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAll() => _aliasSearchDbAction.Search(isReturnAllIfEmpty: true);

    ///<inheritdoc/>
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters() => _getAllAliasDbAction.GetAllAliasWithAdditionalParameters();

    ///<inheritdoc/>
    public AliasQueryResult GetByIdAndName(long id, string name) => _aliasDbAction.GetByIdAndName(id, name);

    ///<inheritdoc/>
    public IEnumerable<SelectableAliasQueryResult> GetDoubloons()
    {
        const string sql = $"""
                           select
                               id        as {nameof(SelectableAliasQueryResult.Id)},
                               notes     as {nameof(SelectableAliasQueryResult.Description)},
                               file_name as {nameof(SelectableAliasQueryResult.FileName)},
                               arguments as {nameof(SelectableAliasQueryResult.Parameters)},
                               name      as {nameof(SelectableAliasQueryResult.Name)},
                               icon      as {nameof(SelectableAliasQueryResult.Icon)},
                               thumbnail as {nameof(SelectableAliasQueryResult.Thumbnail)}
                           from
                               data_doubloons_v
                           order by file_name
                           """;
        var results = DB.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
        var r = _converter.ToSelectableQueryResult(results);
        return r;
    }

    /// <summary>
    /// Get the a first alias with the exact name. In case of multiple aliases
    /// with same name, the one with greater counter is selected.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    public AliasQueryResult GetExact(string name) => _aliasDbAction.GetExact(name);

    ///<inheritdoc/>
    public IEnumerable<string> GetExistingAliases(IEnumerable<string> aliasesToCheck, long idAlias)
    {
        const string  sql = """
                            select name
                            from alias_name
                            where 
                                name in @names
                                and id_alias != @idAlias
                            """;
        return DB.WithinTransaction(tx => tx.Connection!.Query<string>(sql, new { names = aliasesToCheck, idAlias }));
    }

    ///<inheritdoc/>
    public IEnumerable<SelectableAliasQueryResult> GetInvalidAliases()
    {
        var result = GetAll().Where(a => MacroValidator.IsMacroFormat(a.FileName) && MacroValidator.IsValid(a.FileName))
                             .ToArray();
        return _converter.ToSelectableQueryResult(result);
    }

    ///<inheritdoc/>
    public KeywordUsage GetKeyword(string name) => _aliasDbAction.GetHiddenKeyword(name);

    ///<inheritdoc/>
    public IEnumerable<QueryResult> GetMostUsedAliases()
    {
        const string sql = @$"
                select
	                keywords   as {nameof(DisplayUsageQueryResult.Name)},
                    exec_count as {nameof(DisplayUsageQueryResult.Count)}
                from
                    stat_execution_count_v
                order
                    by exec_count desc";
        return DB.WithinTransaction(tx => tx.Connection!.Query<DisplayUsageQueryResult>(sql));
    }

    ///<inheritdoc/>
    public IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per)
    {
        var action = new HistoryDbAction(DB);
        return per switch
        {
            Per.Hour      => action.PerHour(),
            Per.Day       => action.PerDay(),
            Per.DayOfWeek => action.PerDayOfWeek(),
            Per.Month     => action.PerMonth(),
            _             => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level")
        };
    }

    ///<inheritdoc/>
    public void Hydrate(QueryResult queryResult)
    {
        const string sql =
            """
            select
            a.id        as id,
               count(a.id) as count
            from
            	alias a
                inner join alias_name  an on a.id = an.id_alias
                inner join alias_usage au on a.id = au.id_alias
            where an.name = @name
            group by a.id;
            """;

        var result = DB.WithinTransaction(
            tx => tx.Connection!
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

        const string sql = """
                           select
                               id       as id,
                               name     as name,
                               argument as parameter
                           from alias_argument
                           where id_alias = @idAlias
                           """;
        var parameters =
            DB.WithinTransaction(tx => tx.Connection!.Query<QueryResultAdditionalParameters>(sql, new { idAlias = alias.Id }));
        alias.AdditionalParameters = new(parameters);
    }

    ///<inheritdoc/>
    public void HydrateMacro(QueryResult alias)
    {
        const string sql = $"""
                            select
                            	a.id        as {nameof(QueryResult.Id)},
                                count(a.id) as {nameof(QueryResult.Count)}
                            from
                            	alias a
                                inner join alias_usage au on a.id = au.id_alias
                            where
                            	file_name like @name
                            group by
                            	a.id;
                            """;

        DB.WithinTransaction(
            tx =>
            {
                var results = tx.Connection!.Query<dynamic>(sql, new { name = alias.Name });

                var enumerable = results as dynamic[] ?? results.ToArray();
                if (enumerable.Length != 1) return;

                var item = enumerable.ElementAt(0);
                alias.Id = item.Id;
                alias.Count = (int)item.Count;
            }
        );
    }

    ///<inheritdoc/>
    public IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result) => _aliasDbAction.RefreshUsage(result);

    ///<inheritdoc/>
    public void Remove(AliasQueryResult alias) => _aliasDbAction.Remove(alias);

    ///<inheritdoc/>
    public void RemoveMany(IEnumerable<AliasQueryResult> aliases)
    {
        var list = aliases as AliasQueryResult[] ?? aliases.ToArray();
        _logger.LogInformation("Removing {Length} alias(es)", list.Length);
        _aliasDbAction.RemoveMany(list);
    }

    /// <inheritdoc />
    public void SaveOrUpdate(ref AliasQueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        ArgumentNullException.ThrowIfNull(alias.Synonyms);
        ArgumentNullException.ThrowIfNull(alias.Id);

        alias.SanitizeSynonyms();

        using var _ = _logger.BeginSingleScope("UpdatedAlias", alias);

        switch (alias.Id)
        {
            case 0 when !_aliasDbAction.HasNames(alias):
                _aliasDbAction.Create(ref alias);
                _logger.LogInformation("Created new alias {AliasName}", alias.Name);
                break;

            case > 0:
                _logger.LogInformation("Updating alias {AliasName}", alias.Name);
                _aliasDbAction.Update(alias);
                break;
        }

        // Reset state after save
        alias.SynonymsWhenLoaded = alias.Synonyms;
    }

    ///<inheritdoc/>
    public IEnumerable<AliasQueryResult> Search(string name, bool isReturnAllIfEmpty = false) => _aliasSearchDbAction.Search(name, isReturnAllIfEmpty);

    ///<inheritdoc/>
    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria) => _aliasSearchDbAction.SearchAliasWithAdditionalParameters(criteria);

    ///<inheritdoc/>
    public ExistingNameResponse SelectNames(string[] names) => _aliasDbAction.SelectNames(names);

    ///<inheritdoc/>
    public void SetUsage(QueryResult alias)
    {
        if (alias is null)
        {
            _logger.LogInformation("Impossible to set usage: alias is null");
            return;
        }

        if (alias.Name.IsNullOrEmpty())
        {
            _logger.LogInformation("Impossible to set usage: alias name is empty. Alias: {@Alias}", alias);
            return;
        }

        _setUsageDbAction.SetUsage(ref alias);
    }

    ///<inheritdoc/>
    public void UpdateThumbnails(IEnumerable<AliasQueryResult> aliases) => _aliasDbAction.UpdateThumbnails(aliases);

    #endregion
}