using System.Text.RegularExpressions;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteAliasRepository : SQLiteRepositoryBase, IAliasRepository
{
    #region Fields

    private readonly IMappingService _converter;
    private readonly IDbActionFactory _dbActionFactory;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteAliasRepository> _logger;

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
                                a.icon       as {nameof(SelectableAliasQueryResult.Icon)}
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
    public IEnumerable<AliasQueryResult> GetAll() => Db.WithinTransaction(tx => _dbActionFactory.SearchManagement.Search(tx, isReturnAllIfEmpty: true));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters() => Db.WithinTransaction(tx => _getAllAliasDbAction.GetAllAliasWithAdditionalParameters(tx));

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetBrokenAliases()
    {
        const string sql = $"""
                            select 
                                a.id        as {nameof(SelectableAliasQueryResult.Id)},
                                a.notes     as {nameof(SelectableAliasQueryResult.Description)},
                                a.file_name as {nameof(SelectableAliasQueryResult.FileName)},
                                a.arguments as {nameof(SelectableAliasQueryResult.Parameters)},
                                an.name     as {nameof(SelectableAliasQueryResult.Name)},
                                a.icon      as {nameof(SelectableAliasQueryResult.Icon)}
                            from 
                                alias a
                                inner join alias_name an on a.id = an.id_alias
                            where
                                deleted_at is null
                            	and (
                                    file_name like 'c:\%'
                            	    or file_name like 'd:\%'
                            	    or file_name like 'e:\%'
                            	    or file_name like 'f:\%'
                            	    or file_name like 'g:\%'
                            	    or file_name like 'h:\%'
                            	    or file_name like 'i:\%'
                            	)
                            """;
        var aliases = Db.WithinTransaction(tx => tx!.Connection!.Query<SelectableAliasQueryResult>(sql));
        return aliases.Where(e => !File.Exists(e.FileName))
                      .ToArray();
    }

    /// <inheritdoc />
    public AliasQueryResult GetById(long id) => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.GetById(tx, id));

    /// <inheritdoc />
    public Dictionary<string, (long Id, int Counter)> GetHiddenCounters() => Db.WithinTransaction(
        tx =>
        {
            const string sql = """
                               select 
                                    a.Id                    as Id,
                                    an.name                 as AliasName,
                                    ifnull(a.exec_count, 0) as Counter
                               from 
                                    alias a
                                    inner join alias_name an on a.id = an.id_alias
                               where 
                               	    a.hidden is true
                                    and a.deleted_at is null
                                    and an.name = a.file_name
                               order by a.id desc
                               """;
            var dictionary = tx.Connection!.Query<dynamic>(sql)
                               .Select(e => new { Key = e.AliasName, Value = ((long)e.Id, (int)e.Counter) })
                               .ToDictionary(e => (string)e.Key, e => e.Value);
            return dictionary;
        }
    );

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetDeletedAlias()
    {
        const string sql = $"""
                            select 
                                a.id        as {nameof(SelectableAliasQueryResult.Id)},
                                a.notes     as {nameof(SelectableAliasQueryResult.Description)},
                                a.file_name as {nameof(SelectableAliasQueryResult.FileName)},
                                a.arguments as {nameof(SelectableAliasQueryResult.Parameters)},
                                an.name     as {nameof(SelectableAliasQueryResult.Name)},
                                a.icon      as {nameof(SelectableAliasQueryResult.Icon)}
                            from 
                                alias a
                                inner join alias_name an on a.id = an.id_alias
                            where
                                deleted_at is not null;
                            """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
    }

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
                                icon      as {nameof(SelectableAliasQueryResult.Icon)}
                            from
                                data_doubloons_v
                            order by file_name
                            """;
        var results = Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
        var r = _converter.ToSelectableQueryResult(results);
        return r;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetExistingDeletedAliases(IEnumerable<string> aliasesToCheck, long idAlias)
    {
        const string  sql = """
                            select name
                            from
                                alias_name an
                                inner join alias a on a.id = an.id_alias
                            where 
                                name in @names
                                and a.deleted_at is not null
                                and id_alias != @idAlias
                            """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<string>(sql, new { names = aliasesToCheck, idAlias }));
    }
    
    /// <inheritdoc />
    public IEnumerable<string> GetExistingAliases(IEnumerable<string> aliasesToCheck, long idAlias)
    {
        const string  sql = """
                            select an.name
                            from 
                                alias_name an
                                inner join alias a on a.id = an.id_alias
                            where 
                                an.name in @names
                                and a.deleted_at is null
                                and an.id_alias != @idAlias
                            """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<string>(sql, new { names = aliasesToCheck, idAlias }));
    }

    /// <inheritdoc />
    public IEnumerable<UsageQueryResult> GetMostUsedAliases()
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
    public IEnumerable<UsageQueryResult> GetMostUsedAliasesByYear(int year)
    {
        const string sql = $"""
                            select
                            	keywords   as {nameof(UsageQueryResult.Name)},
                                exec_count as {nameof(UsageQueryResult.Count)},
                                year       as {nameof(UsageQueryResult.Year)}
                            from
                                stat_execution_count_by_year_v
                            where year = @year
                            order
                                by exec_count desc
                            """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<UsageQueryResult>(sql, new { year = $"{year}" }));
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
            Per.Year      => action.PerYear(),
            _             => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level")
        };
    }


    /// <inheritdoc />
    public IEnumerable<int> GetYearsWithUsage()
    {
        const string sql = "select distinct strftime('%Y', time_stamp) from alias_usage";
        return Db.WithinTransaction(tx => tx.Connection!.Query<int>(sql));
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
                var item = tx.Connection!.Query<dynamic>(sql, new { name = alias.Name }).FirstOrDefault();

                if (item is null) return;

                alias.Id = item.Id;
                alias.Count = (int)item.Count;
            }
        );
    }

    /// <inheritdoc />
    public void Remove(AliasQueryResult alias) => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.LogicalRemove(tx, alias));

    /// <inheritdoc />
    public void Remove(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(
        tx =>
        {
            var list = aliases as AliasQueryResult[] ?? aliases.ToArray();
            _logger.LogInformation("Removing {Length} alias(es)", list.Length);
            _dbActionFactory.AliasManagement.LogicalRemove(tx, list);
        }
    );

    /// <inheritdoc />
    public void Restore(IEnumerable<SelectableAliasQueryResult> aliases)
    {
        const string sql = """
                           update alias
                           set
                               deleted_at = null,
                               hidden     = 0 
                           where id in @selectedAliases;
                           """;
        var selectedAliases = aliases.Select(e => e.Id).ToArray();
        Db.WithinTransaction(tx => tx.Connection!.Query(sql, new { selectedAliases  }));
    }

    /// <inheritdoc />
    public void SaveOrUpdate(ref AliasQueryResult alias) => Db.WithinTransaction((tx, current) =>  _dbActionFactory.SaveManagement.SaveOrUpdate(tx, ref current), alias);

    /// <inheritdoc />
    public void SaveOrUpdate(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(_dbActionFactory.SaveManagement.SaveOrUpdate, aliases);

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> Search(string name, bool isReturnAllIfEmpty = false) => Db.WithinTransaction(tx => _dbActionFactory.SearchManagement.Search(tx, name, isReturnAllIfEmpty));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria) => Db.WithinTransaction(tx => _dbActionFactory.SearchManagement.SearchAliasWithAdditionalParameters(tx, criteria));

    /// <inheritdoc />
    public ExistingNameResponse SelectNames(string[] names) => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.SelectNames(tx, names));

    /// <inheritdoc />
    public void SetHiddenAliasUsage(QueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);

        var usage = Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.GetHiddenAlias(tx, alias.Name));
        if (usage is null) return;

        alias.Id = usage.Id;
        alias.Count = usage.Count;
    }

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

            _dbActionFactory.UsageManagement.SetUsage(tx, ref alias);
        }
    );

    #endregion
}