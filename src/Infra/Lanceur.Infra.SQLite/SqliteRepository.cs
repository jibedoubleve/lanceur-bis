using System.Reflection;
using Dapper;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Macros;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite;

public class SQLiteRepository : SQLiteRepositoryBase, IDbRepository
{
    #region Fields

    private readonly IMappingService _converter;
    private readonly IDbActionFactory _dbActionFactory;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteRepository> _logger;
    private static readonly MacroValidator MacroValidator = new(Assembly.GetAssembly(typeof(GuidMacro)));

    #endregion

    #region Constructors

    public SQLiteRepository(
        IDbConnectionManager manager,
        ILoggerFactory logFactory,
        IMappingService converter,
        IDbActionFactory dbActionFactory
    ) : base(manager)
    {
        ArgumentNullException.ThrowIfNull(logFactory);
        ArgumentNullException.ThrowIfNull(converter);

        _logger = logFactory.GetLogger<SQLiteRepository>();
        _converter = converter;
        _getAllAliasDbAction = new(manager);
        _dbActionFactory = dbActionFactory;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<AdditionalParameter> GetAdditionalParameter(IEnumerable<long> ids)
    {
        const string sql = """
                           select 
                           	id_alias as AliasId,
                           	id       as Id,
                           	name     as Name,
                           	argument as Parameter
                           from alias_argument	
                           where id_alias in @ids
                           """;
        return DB.WithinTransaction(tx => tx!.Connection!.Query<AdditionalParameter>(sql, new { ids }));
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAll() => DB.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().Search(tx, isReturnAllIfEmpty: true));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters() => DB.WithinTransaction(tx => _getAllAliasDbAction.GetAllAliasWithAdditionalParameters(tx));

    /// <inheritdoc />
    public AliasQueryResult GetByIdAndName(long id, string name) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetByIdAndName(id, name, tx));

    /// <inheritdoc />
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
    ///     Get the a first alias with the exact name. In case of multiple aliases
    ///     with same name, the one with greater counter is selected.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    public AliasQueryResult GetExact(string name) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetExact(name, tx));

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetInvalidAliases()
    {
        var result = GetAll()
                     .Where(a => MacroValidator.IsMacroFormat(a.FileName) && MacroValidator.IsValid(a.FileName))
                     .ToArray();
        return _converter.ToSelectableQueryResult(result);
    }

    /// <inheritdoc />
    public KeywordUsage GetKeyword(string name) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetHiddenKeyword(name, tx));

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetMostUsedAliases()
    {
        const string sql = $"""
                            select
                            	keywords   as {nameof(UsageQueryResult.Name)},
                                exec_count as {nameof(UsageQueryResult.Count)}
                            from
                                stat_execution_count_v
                            order
                                by exec_count desc
                            """;
        return DB.WithinTransaction(tx => tx.Connection!.Query<UsageQueryResult>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per)
    {
        var action = new HistoryDbAction(DB);
        return per switch
        {
            Per.HourOfDay => action.PerHour(),
            Per.Day       => action.PerDay(),
            Per.DayOfWeek => action.PerDayOfWeek(),
            Per.Month     => action.PerMonth(),
            _             => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level")
        };
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void HydrateAlias(AliasQueryResult alias)
    {
        if (alias is null) return;

        const string sqlArguments = """
                                    select
                                        id       as id,
                                        name     as name,
                                        argument as parameter
                                    from alias_argument
                                    where id_alias = @idAlias
                                    """;
        const string sqlSynonyms = """
                                   select name
                                   from alias_name
                                   where id_alias = @idAlias
                                   """;
        var (parameters, synonyms) =
            DB.WithinTransaction(
                tx =>
                {
                    var parameters = tx.Connection!.Query<AdditionalParameter>(sqlArguments, new { idAlias = alias.Id });
                    var synonyms = tx.Connection!.Query<string>(sqlSynonyms, new { idAlias = alias.Id });

                    return (parameters, synonyms);
                }
            );

        alias.AdditionalParameters = new(parameters);
        alias.Synonyms = string.Join(", ", synonyms);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().RefreshUsage(result, tx));

    /// <inheritdoc />
    public void Remove(AliasQueryResult alias) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().Remove(alias, tx));

    /// <inheritdoc />
    public void RemoveMany(IEnumerable<AliasQueryResult> aliases) => DB.WithinTransaction(
        tx =>
        {
            var list = aliases as AliasQueryResult[] ?? aliases.ToArray();
            _logger.LogInformation("Removing {Length} alias(es)", list.Length);
            _dbActionFactory.AliasDbAction().RemoveMany(list, tx);
        }
    );

    /// <inheritdoc />
    public void SaveOrUpdate(ref AliasQueryResult alias)
    {
        _ = DB.WithinTransaction(
            (tx, current) =>
            {
                ArgumentNullException.ThrowIfNull(current);
                ArgumentNullException.ThrowIfNull(current.Synonyms);
                ArgumentNullException.ThrowIfNull(current.Id);

                current.SanitizeSynonyms();
                var action = _dbActionFactory.AliasDbAction();

                using var _ = _logger.BeginSingleScope("UpdatedAlias", current);

                switch (current.Id)
                {
                    case 0 when !action.HasNames(current, tx):
                        action.Create(tx, ref current);
                        _logger.LogInformation("Created new alias {AliasName}", current.Name);
                        break;

                    case > 0:
                        _logger.LogInformation("Updating alias {AliasName}", current.Name);
                        action.Update(current, tx);
                        break;
                }

                // Reset state after save
                current.SynonymsWhenLoaded = current.Synonyms;

                return current;
            },
            alias
        );
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> Search(string name, bool isReturnAllIfEmpty = false) => DB.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().Search(tx, name, isReturnAllIfEmpty));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria) => DB.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().SearchAliasWithAdditionalParameters(tx, criteria));

    /// <inheritdoc />
    public ExistingNameResponse SelectNames(string[] names) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().SelectNames(names, tx));

    /// <inheritdoc />
    public void SetUsage(QueryResult alias) => DB.WithinTransaction(
        tx =>
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

            _dbActionFactory.SetUsageDbAction().SetUsage(tx, ref alias);
        }
    );

    /// <inheritdoc />
    public void UpdateThumbnails(IEnumerable<AliasQueryResult> aliases) => DB.WithinTransaction(tx => _dbActionFactory.AliasDbAction().UpdateThumbnails(aliases, tx));

    #endregion
}