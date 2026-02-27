using System.Text.RegularExpressions;
using Dapper;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteAliasRepository : SQLiteRepositoryBase, IAliasRepository
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteAliasRepository> _logger;

    private static readonly Regex RegexSelectUrl = new(
        @"(www?|http?|https?|ftp):\/\/[^\s/$.?#].[^\s]*$|^[a-zA-Z0-9-]+\.[a-zA-Z]{2,6}(\.[a-zA-Z]{2,})?$"
    );

    #endregion

    #region Constructors

    public SQLiteAliasRepository(
        IDbConnectionManager manager,
        ILoggerFactory logFactory,
        IDbActionFactory dbActionFactory
    ) : base(manager)
    {
        ArgumentNullException.ThrowIfNull(logFactory);

        _logger = logFactory.GetLogger<SQLiteAliasRepository>();
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
                                a.id          as {nameof(SelectableAliasQueryResult.Id)},
                                a.notes       as {nameof(SelectableAliasQueryResult.Description)},
                                a.file_name   as {nameof(SelectableAliasQueryResult.FileName)},
                                a.arguments   as {nameof(SelectableAliasQueryResult.Parameters)},
                                an.name       as {nameof(SelectableAliasQueryResult.Name)},
                                sub.synonyms  as {nameof(SelectableAliasQueryResult.Synonyms)},
                                a.icon        as {nameof(SelectableAliasQueryResult.Icon)},
                                a.thumbnail   as {nameof(SelectableAliasQueryResult.Thumbnail)},
                                a.exec_count  as {nameof(SelectableAliasQueryResult.Count)},
                                lu.last_usage as {nameof(SelectableAliasQueryResult.LastUsedAt)}
                            from 
                                alias a
                                inner join alias_name an on a.id = an.id_alias
                                left join data_last_usage_v lu on a.id = lu.id_alias
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
                                and a.deleted_at is null
                            order by an.name
                            """;
        var results = Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));


        results = results
                  .Where(e => !RegexSelectUrl.IsMatch(e.FileName)
                  ) // Excluding all aliases that serve as shortcuts for URLs
                  .Where(e => !e.FileName.StartsWith("package:"))  // Excluding all packaged applications
                  .ToArray();
        return results.ToSelectableQueryResult();
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAll()
        => Db.WithinTransaction(tx => _dbActionFactory.SearchManagement.Search(tx, isReturnAllIfEmpty: true));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters()
        => Db.WithinTransaction(tx => _getAllAliasDbAction.GetAllAliasWithAdditionalParameters(tx));

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetBrokenAliases()
    {
        const string sql = $"""
                            select 
                                a.id         as {nameof(SelectableAliasQueryResult.Id)},
                                a.notes      as {nameof(SelectableAliasQueryResult.Description)},
                                a.file_name  as {nameof(SelectableAliasQueryResult.FileName)},
                                a.arguments  as {nameof(SelectableAliasQueryResult.Parameters)},
                                an.name      as {nameof(SelectableAliasQueryResult.Name)},
                                a.icon       as {nameof(SelectableAliasQueryResult.Icon)},
                                a.thumbnail  as {nameof(SelectableAliasQueryResult.Thumbnail)},
                                a.exec_count as {nameof(SelectableAliasQueryResult.Count)},
                                l.last_usage as {nameof(SelectableAliasQueryResult.LastUsedAt)}

                            from 
                                alias a
                                inner join alias_name an on a.id = an.id_alias
                                left join data_last_usage_v l on a.id = l.id_alias
                            where
                                deleted_at is null
                                and a.deleted_at is null
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
    public AliasQueryResult GetById(long id)
        => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.GetById(tx, id));

    /// <inheritdoc />
    public IEnumerable<DateTime> GetDaysWithHistory(DateTime day)
    {
        const string sql = """
                           select 
                              date(time_stamp) as day
                           from alias_usage
                           where  strftime('%m-%Y', time_stamp) = strftime('%m-%Y', @date)
                           group by strftime('%d-%m-%Y', time_stamp)
                           order by strftime('%Y-%m-%d', time_stamp);
                           """;
        return Db.WithConnection(c => c.Query<DateTime>(sql, new { date = day.Date }));
    }

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetDeletedAlias()
    {
        const string sql = $"""
                            select 
                                a.id         as {nameof(SelectableAliasQueryResult.Id)},
                                a.notes      as {nameof(SelectableAliasQueryResult.Description)},
                                a.file_name  as {nameof(SelectableAliasQueryResult.FileName)},
                                a.arguments  as {nameof(SelectableAliasQueryResult.Parameters)},
                                an.name      as {nameof(SelectableAliasQueryResult.Name)},
                                a.icon       as {nameof(SelectableAliasQueryResult.Icon)},
                                a.thumbnail  as {nameof(SelectableAliasQueryResult.Thumbnail)},
                                a.exec_count as {nameof(SelectableAliasQueryResult.Count)},
                                l.last_usage as {nameof(SelectableAliasQueryResult.LastUsedAt)}
                            from 
                                alias a
                                inner join alias_name an on a.id = an.id_alias
                                left join data_last_usage_v l on a.id = l.id_alias
                            where
                                deleted_at is not null
                            order by an.name;
                            """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetDoubloons()
    {
        const string sql = $"""
                            select
                                id           as {nameof(SelectableAliasQueryResult.Id)},
                                notes        as {nameof(SelectableAliasQueryResult.Description)},
                                file_name    as {nameof(SelectableAliasQueryResult.FileName)},
                                arguments    as {nameof(SelectableAliasQueryResult.Parameters)},
                                name         as {nameof(SelectableAliasQueryResult.Name)},
                                icon         as {nameof(SelectableAliasQueryResult.Icon)},
                                thumbnail    as {nameof(SelectableAliasQueryResult.Thumbnail)},
                                exec_count   as {nameof(SelectableAliasQueryResult.Count)},
                                last_usage   as {nameof(SelectableAliasQueryResult.LastUsedAt)}
                            from
                                data_doubloons_v a
                                left join data_last_usage_v b on a.id = b.id_alias
                            order by file_name
                            """;
        var results = Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
        var r = results.ToSelectableQueryResult();
        return r;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetExistingAliases(IEnumerable<string> names, long idAlias)
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
        return Db.WithinTransaction(tx => tx.Connection!.Query<string>(sql, new {   names, idAlias }));
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
    public DateTime? GetFirstHistory()
    {
        const string sql = "select date(min(time_stamp)) from alias_usage";
        return Db.WithConnection(c => c.Query<DateTime>(sql).FirstOrDefault());
    }

    /// <inheritdoc />
    public Dictionary<string, (long Id, int Counter)> GetHiddenCounters() => Db.WithinTransaction(tx =>
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
    public IEnumerable<SelectableAliasQueryResult> GetInactiveAliases(int months, DateTime? startThreshold = null)
    {
        var nowDate = startThreshold ?? DateTime.Today;

        const int threshold = 12 * 30; // 30 years max in the past...
        if (months >= threshold) months = threshold;
        var sql = $"""
                   select 
                       a.id_alias           as {nameof(SelectableAliasQueryResult.Id)},
                       c.notes              as {nameof(SelectableAliasQueryResult.Description)},
                       c.file_name          as {nameof(SelectableAliasQueryResult.FileName)},
                       c.arguments          as {nameof(SelectableAliasQueryResult.Parameters)},
                       group_concat(b.name) as {nameof(SelectableAliasQueryResult.Name)},
                       c.icon               as {nameof(SelectableAliasQueryResult.Icon)},
                       c.thumbnail          as {nameof(SelectableAliasQueryResult.Thumbnail)},
                       a.last_used          as {nameof(SelectableAliasQueryResult.LastUsedAt)},
                       e.count              as {nameof(SelectableAliasQueryResult.Count)}
                   from 
                       (select 
                           id_alias        as id_alias,
                           max(time_stamp) as last_used
                       from 
                           alias_usage  
                       group by id_alias)    a
                       inner join alias_name b          on a.id_alias = b.id_alias
                       inner join alias      c          on a.id_alias = c.id
                       left join stat_usage_per_app_v e on a.id_alias = e.id_alias
                   where 
                        a.last_used < date(@nowDate, '-{months} months')
                        and deleted_at is null
                   group by a.id_alias
                   order by last_used asc
                   """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql, new { nowDate }));
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
    public IEnumerable<SelectableAliasQueryResult> GetRarelyUsedAliases(int threshold)
    {
        var sql = $"""
                   select 
                       a.id_alias           as {nameof(SelectableAliasQueryResult.Id)},
                       c.notes              as {nameof(SelectableAliasQueryResult.Description)},
                       c.file_name          as {nameof(SelectableAliasQueryResult.FileName)},
                       c.arguments          as {nameof(SelectableAliasQueryResult.Parameters)},
                       group_concat(b.name) as {nameof(SelectableAliasQueryResult.Name)},
                       c.icon               as {nameof(SelectableAliasQueryResult.Icon)},
                       c.thumbnail          as {nameof(SelectableAliasQueryResult.Thumbnail)},
                       a.count              as {nameof(SelectableAliasQueryResult.Count)},
                       d.last_used          as {nameof(SelectableAliasQueryResult.LastUsedAt)}
                   from 
                       stat_usage_per_app_v a
                       inner join alias_name b on a.id_alias = b.id_alias
                       inner join alias c on c.id = a.id_alias
                       left join (select
                                      id_alias        as id_alias,
                                      max(time_stamp) as last_used
                                  from
                                      alias_usage
                                  group by id_alias) d on d.id_alias = a.id_alias
                   where 
                       count < {threshold}
                       and c.deleted_at is null
                   group by b.id_alias
                   order by a.count desc
                   """;
        return Db.WithinTransaction(tx => tx.Connection!.Query<SelectableAliasQueryResult>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> GetUnusedAliases()
    {
        const string sql = $"""
                            select
                                id         as {nameof(SelectableAliasQueryResult.Id)},
                                notes      as {nameof(SelectableAliasQueryResult.Description)},
                                file_name  as {nameof(SelectableAliasQueryResult.FileName)},
                                arguments  as {nameof(SelectableAliasQueryResult.Parameters)},
                                name       as {nameof(SelectableAliasQueryResult.Name)},
                                icon       as {nameof(SelectableAliasQueryResult.Icon)},
                                thumbnail  as {nameof(SelectableAliasQueryResult.Thumbnail)},
                                last_usage as {nameof(SelectableAliasQueryResult.LastUsedAt)}
                            from (
                                select 
                                    an.id_alias,
                                    group_concat(an.name, ', ') as name
                                from (
                                    select 
                                        a.id  as id_alias        
                                    from 
                                        alias a
                                        left join alias_usage b on a.id = b.id_alias
                                    where 
                                        b.id_alias is null
                                        and a.deleted_at is null
                                 ) t
                                    inner join alias_name an on an.id_alias = t.id_alias
                                group by t.id_alias
                            ) tt 
                                inner join alias aa on aa.id = tt.id_alias
                                inner join data_last_usage_v l on aa.id = l.id_alias
                            order by name 
                            """;
        return Db.WithConnection(conn => conn.Query<SelectableAliasQueryResult>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per, int? year = null)
    {
        var action = new HistoryDbAction(Db);
        return per switch
        {
            Per.HourOfDay => action.PerHour(year),
            Per.Day       => action.PerDay(year),
            Per.DayOfWeek => action.PerDayOfWeek(year),
            Per.Month     => action.PerMonth(year),
            Per.Year      => action.PerYear(year),
            _             => throw new NotSupportedException($"Cannot retrieve the usage at the '{per}' level")
        };
    }

    /// <inheritdoc />
    public IEnumerable<AliasUsageItem> GetUsageFor(DateTime selectedDay)
    {
        const string sql = $"""
                            select
                                a.id        as {nameof(AliasUsageItem.Id)},
                                sy.synonyms as {nameof(AliasUsageItem.Name)},
                                time_stamp  as {nameof(AliasUsageItem.Timestamp)},
                                a.file_name as {nameof(AliasUsageItem.FileName)},
                                a.Icon      as {nameof(AliasUsageItem.Icon)},
                                a.thumbnail as {nameof(AliasUsageItem.Thumbnail)}
                            from 
                                alias_usage au
                                left join data_alias_synonyms_v sy on sy.id_alias = au.id_alias
                                inner join alias a on a.id = au.id_alias
                            where date(time_stamp) = date(@selectedDay)
                            order by au.time_stamp
                            """;
        return Db.WithConnection(c => c.Query<AliasUsageItem>(sql, new { selectedDay }));
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
            Db.WithinTransaction(tx =>
                {
                    var parameters = tx.Connection!.Query<AdditionalParameter>(
                        sqlArguments,
                        new { idAlias = alias.Id }
                    );
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

        Db.WithinTransaction(tx =>
            {
                var item = tx.Connection!.Query<dynamic>(sql, new { name = alias.Name }).FirstOrDefault();

                if (item is null) return;

                alias.Id = item.Id;
                alias.Count = (int)item.Count;
            }
        );
    }

    /// <inheritdoc />
    public void MergeHistory(IEnumerable<long> fromAliases, long toAlias) => Db.WithinTransaction(tx =>
        {
            const string sql = """
                               update alias_usage 
                               set id_alias = @destinationId
                               where id_alias in @sourceIds
                               """;
            tx.Connection!.Execute(sql, new { destinationId = toAlias, sourceIds = fromAliases });
        }
    );

    /// <inheritdoc />
    public void Remove(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(tx =>
        {
            var list = aliases as AliasQueryResult[] ?? aliases.ToArray();
            _logger.LogInformation("Hard remove of {Count} alias(es) from database", list.Length);
            foreach (var item in list) _dbActionFactory.AliasManagement.Remove(tx, item);
        }
    );

    /// <inheritdoc />
    public void RemoveLogically(AliasQueryResult alias)
        => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.LogicalRemove(tx, alias));

    /// <inheritdoc />
    public void RemoveLogically(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(tx =>
        {
            var list = aliases as AliasQueryResult[] ?? aliases.ToArray();
            _logger.LogInformation("Logical remove of {Length} alias(es)", list.Length);
            _dbActionFactory.AliasManagement.LogicalRemove(tx, list);
        }
    );

    /// <inheritdoc />
    public void RemovePermanently(SelectableAliasQueryResult[] aliases)
    {
        const string delUsage = "delete from alias_usage where id_alias = @idAlias;";
        const string delNames = "delete from alias_name where id_alias = @idAlias;";
        const string delArguments = "delete from alias_argument where id_alias = @idAlias;";
        const string delAlias = "delete from alias where id = @idAlias;";

        Db.WithinTransaction(tx =>
            {
                foreach (var idAlias in aliases.Select(e => e.Id).ToArray())
                {
                    tx.Connection!.Execute(delUsage, new { idAlias });
                    tx.Connection!.Execute(delNames, new { idAlias });
                    tx.Connection!.Execute(delArguments, new { idAlias });
                    tx.Connection!.Execute(delAlias, new { idAlias });
                }
            }
        );
    }

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
    public void Restore(AliasQueryResult alias) => Db.WithinTransaction(tx =>
        {
            const string sql = """
                               update alias 
                               set
                                    deleted_at = null,
                                    hidden     = 0
                               where id = @id
                               """;
            tx.Connection!.Execute(sql, new { id = alias.Id });
        }
    );

    /// <inheritdoc />
    public void SaveOrUpdate(ref AliasQueryResult alias) => Db.WithinTransaction(
        (tx, current) =>  _dbActionFactory.SaveManagement.SaveOrUpdate(tx, ref current),
        alias
    );

    /// <inheritdoc />
    public void SaveOrUpdate(IEnumerable<AliasQueryResult> aliases) => Db.WithinTransaction(
        _dbActionFactory.SaveManagement.SaveOrUpdate,
        aliases
    );

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> Search(string name, bool isReturnAllIfEmpty = false)
        => Db.WithinTransaction(tx => _dbActionFactory.SearchManagement.Search(tx, name, isReturnAllIfEmpty));

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria) => Db.WithinTransaction(tx
        => _dbActionFactory.SearchManagement.SearchAliasWithAdditionalParameters(tx, criteria)
    );

    /// <inheritdoc />
    public ExistingNameResponse SelectNames(string[] names)
        => Db.WithinTransaction(tx => _dbActionFactory.AliasManagement.SelectNames(tx, names));

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
    public void SetUsage(QueryResult alias) => Db.WithinTransaction(tx =>
        {
            if (alias is null)
            {
                _logger.LogWarning("Impossible to set usage: alias is null");
                return;
            }

            if (alias.Name.IsNullOrEmpty())
            {
                _logger.LogWarning("Impossible to set usage: alias name is empty. Alias: {@Alias}", alias);
                return;
            }

            _dbActionFactory.UsageManagement.SetUsage(tx, ref alias);
        }
    );

    /// <inheritdoc />
    public void UpdateThumbnail(AliasQueryResult alias) => Db.WithinTransaction(tx =>
        {
            const string sql = "update alias set thumbnail = @thumbnail where id = @id;";
            tx.Connection!.Execute(sql, new { id = alias.Id, thumbnail = alias.Thumbnail });
        }
    );

    #endregion
}