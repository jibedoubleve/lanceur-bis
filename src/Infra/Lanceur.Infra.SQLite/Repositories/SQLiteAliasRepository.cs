using System.Reflection;
using System.Text.RegularExpressions;
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

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteAliasRepository : SQLiteRepositoryBase, IAliasRepository
{
    #region Fields

    private readonly IMappingService _converter;
    private readonly IDbActionFactory _dbActionFactory;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteAliasRepository> _logger;
    private static readonly MacroValidator MacroValidator = new(Assembly.GetAssembly(typeof(GuidMacro)));

    private static readonly Regex RegexSelectUrl = new(@"(www?|http?|https?|ftp):\/\/[^\s/$.?#].[^\s]*$|^[a-zA-Z0-9-]+\.[a-zA-Z]{2,6}(\.[a-zA-Z]{2,})?$");

    #endregion

    #region Constructors

    public SQLiteAliasRepository(
        IDbConnectionManager manager,
        ILoggerFactory logFactory,
        IMappingService converter,
        IDbActionFactory dbActionFactory
    ) : base(manager)
    {
        ArgumentNullException.ThrowIfNull(logFactory);
        ArgumentNullException.ThrowIfNull(converter);

        _logger = logFactory.GetLogger<SQLiteAliasRepository>();
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
        return Db.WithinTransaction(tx => tx!.Connection!.Query<AdditionalParameter>(sql, new { ids }));
    }

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetAliasesWithoutNotes()
    {
        const string sql = $"""
                             select
                                 a.id         as {nameof(SelectableAliasQueryResult.Id)},
                                 a.notes      as {nameof(SelectableAliasQueryResult.Description)},
                                 a.file_name  as {nameof(SelectableAliasQueryResult.FileName)},
                                 a.arguments  as {nameof(SelectableAliasQueryResult.Parameters)},
                                 an.name      as {nameof(SelectableAliasQueryResult.Name)},
                                 sub.synonyms as {nameof(SelectableAliasQueryResult.Synonyms)},
                                 a.icon       as {nameof(SelectableAliasQueryResult.Icon)},
                                 a.thumbnail  as {nameof(SelectableAliasQueryResult.Thumbnail)}
                             from 
                                 alias a
                                 inner join alias_name an on a.id = an.id_alias
                                 inner join (
                                     select 
                                         id_alias,
                                         group_concat(name) as synonyms
                                     from alias_name
                                     group by id_alias
                             ) as sub on sub.id_alias = a.id
                             where
                                 a.notes is null
                                 and a.hidden = 0
                             order by an.name
                             """;
        var results = Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));


        results = results.Where(e => !RegexSelectUrl.IsMatch(e.FileName)) // Excluding all aliases that serve as shortcuts for URLs
                         .Where(e => !e.FileName.StartsWith("package:"))  // Excluding all packaged applications
                         .ToArray();
        return _converter.ToSelectableQueryResult(results);
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAll() => Db.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().Search(tx, isReturnAllIfEmpty: true));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters() => Db.WithinTransaction(tx => _getAllAliasDbAction.GetAllAliasWithAdditionalParameters(tx));

    /// <inheritdoc />
    public AliasQueryResult GetByIdAndName(long id, string name) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetByIdAndName(id, name, tx));

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
        var results = Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
        var r = _converter.ToSelectableQueryResult(results);
        return r;
    }

    /// <summary>
    ///     Get the a first alias with the exact name. In case of multiple aliases
    ///     with same name, the one with greater counter is selected.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    public AliasQueryResult GetExact(string name) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetExact(name, tx));

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
        return Db.WithinTransaction(tx => tx.Connection!.Query<string>(sql, new { names = aliasesToCheck, idAlias }));
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
    public KeywordUsage GetKeyword(string name) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().GetHiddenKeyword(name, tx));

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
        return Db.WithinTransaction(tx => tx.Connection!.Query<UsageQueryResult>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per)
    {
        var action = new HistoryDbAction(Db);
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

        var result = Db.WithinTransaction(
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
            Db.WithinTransaction(
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

        Db.WithinTransaction(
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
    public IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().RefreshUsage(result, tx));

    /// <inheritdoc />
    public void Remove(AliasQueryResult alias) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().Remove(alias, tx));

    /// <inheritdoc />
    public void RemoveMany(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(
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
        var dbAction = _dbActionFactory.AliasSaveDbAction();
        Db.WithinTransaction((tx, current) => dbAction.SaveOrUpdate(tx, ref current), alias);
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> Search(string name, bool isReturnAllIfEmpty = false) => Db.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().Search(tx, name, isReturnAllIfEmpty));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria) => Db.WithinTransaction(tx => _dbActionFactory.AliasSearchDbAction().SearchAliasWithAdditionalParameters(tx, criteria));

    /// <inheritdoc />
    public ExistingNameResponse SelectNames(string[] names) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().SelectNames(names, tx));

    /// <inheritdoc />
    public void SetUsage(QueryResult alias) => Db.WithinTransaction(
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
    public void Update(IEnumerable<AliasQueryResult> aliases)
    {
        var dbAction = _dbActionFactory.AliasSaveDbAction();
        Db.WithinTransaction(dbAction.Update, aliases);
    }

    /// <inheritdoc />
    public void UpdateThumbnails(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(tx => _dbActionFactory.AliasDbAction().UpdateThumbnails(aliases, tx));

    #endregion
}