using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Sqlite.Entities;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur.Infra.SQLite.DbActions;

public class AliasDbAction
{
    #region Fields

    private readonly IDbConnectionManager _db;
    private readonly ILogger<AliasDbAction> _logger;

    #endregion Fields

    #region Constructors

    public AliasDbAction(IDbConnectionManager db, ILoggerFactory logFactory)
    {
        _db = db;
        _logger = logFactory.GetLogger<AliasDbAction>();
    }

    #endregion Constructors

    #region Methods

    private static void UpdateName(AliasQueryResult alias, IDbTransaction tx)
    {
        //Remove all names
        const string sql = @"delete from alias_name where id_alias = @id";
        tx.Connection.Execute(sql, new { id = alias.Id });

        //Recreate new names
        const string sql2 = @"insert into alias_name (id_alias, name) values (@id, @name)";
        foreach (var name in alias.Synonyms.SplitCsv()) tx.Connection.Execute(sql2, new { id = alias.Id, name });
    }

    private void ClearAlias(params long[] ids)
    {
        const string sql = @"
                    delete from alias
                    where id = @id";

        var cnt = _db.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias. Id: {Idd}", cnt, idd);
    }

    private void ClearAliasArgument(params long[] ids)
    {
        const string sql = @"
                delete from alias_argument
                where id_alias = @id";

        var cnt = _db.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias_argument. Id: {IdAliases}", cnt, idd);
    }

    private void ClearAliasName(params long[] ids)
    {
        const string sql = @"
                delete from alias_name
                where id_alias = @id";

        var cnt = _db.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {RemovedCount} row(s) from alias_name. Id: {IdAliases}", cnt, idd);
    }

    private void ClearAliasUsage(params long[] ids)
    {
        const string sql = @"
                delete from alias_usage
                where id_alias = @id;";

        var cnt = _db.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias_usage. Id: {IdAliases}", cnt, idd);
    }

    private void CreateAdditionalParameters(long idAlias, IEnumerable<QueryResultAdditionalParameters> parameters, IDbTransaction tx)
    {
        using var _ = _logger.BeginSingleScope("Parameters", parameters);
        const string sql1 = "delete from alias_argument where id_alias = @idAlias";
        const string sql2 = "insert into alias_argument (id_alias, argument, name) values(@idAlias, @parameter, @name);";

        // Remove existing additional alias parameters
        var deletedRowsCount = tx.Connection.Execute(sql1, new { idAlias });

        // Create alias additional parameters
        var addedRowsCount = tx.Connection.Execute(sql2, parameters.ToEntity(idAlias));

        if (deletedRowsCount > 0 && addedRowsCount == 0) _logger.LogWarning("Deleting {DeletedRowsCount} parameters while adding no new parameters", deletedRowsCount);
    }

    private void CreateAdditionalParameters(AliasQueryResult alias, IDbTransaction tx) => CreateAdditionalParameters(alias.Id, alias.AdditionalParameters, tx);

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
            result.Count = (from item in dbResults
                            where item.Id == result.Id
                            select item.Count).SingleOrDefault();
        return dbResultAr;
    }

    public long Create(ref AliasQueryResult alias)
    {
        const string sqlAlias = @"
                insert into alias (
                    arguments,
                    file_name,
                    notes,
                    run_as,
                    start_mode,
                    working_dir,
                    icon,
                    thumbnail,
                    lua_script,
                    hidden,
                    confirmation_required
                ) values (
                    @arguments,
                    @fileName,
                    @notes,
                    @runAs,
                    @startMode,
                    @workingDirectory,
                    @icon,
                    @thumbnail,
                    @luaScript,
                    @isHidden,
                    @isExecutionConfirmationRequired
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
            alias.Icon,
            alias.Thumbnail,
            alias.LuaScript,
            alias.IsHidden,
            alias.IsExecutionConfirmationRequired
        };

        var csv = alias.Synonyms.SplitCsv();
        var additionalParameters = alias.AdditionalParameters;

        using var _ = _logger.BeginSingleScope("SqlCreateAlias", sqlAlias);
        var id = _db.WithinTransaction(
            tx =>
            {
                var id = tx.Connection.ExecuteScalar<long>(sqlAlias, param);

                // Create synonyms
                const string sqlSynonyms = @"insert into alias_name (id_alias, name) values (@id, @name)";
                foreach (var name in csv) tx.Connection.ExecuteScalar<long>(sqlSynonyms, new { id, name });

                //Create additional arguments
                CreateAdditionalParameters(id, additionalParameters, tx);
                return id;
            }
        );

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
        alias.Id = Create(ref queryResult);
    }

    /// <summary>
    /// Get the a first alias with the exact name.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    /// <remarks>
    /// For optimisation reason, there's no check of doubloons. Bear UI validates and
    /// forbid to insert two aliases with same name.
    /// </remarks>
    public AliasQueryResult GetExact(string name, bool includeHidden = false)
    {
        const string sql = @$"
                select
                    n.Name                  as {nameof(AliasQueryResult.Name)},
                    s.synonyms              as {nameof(AliasQueryResult.Synonyms)},
                    a.Id                    as {nameof(AliasQueryResult.Id)},
                    a.arguments             as {nameof(AliasQueryResult.Parameters)},
                    a.file_name             as {nameof(AliasQueryResult.FileName)},
                    a.notes                 as {nameof(AliasQueryResult.Notes)},
                    a.run_as                as {nameof(AliasQueryResult.RunAs)},
                    a.start_mode            as {nameof(AliasQueryResult.StartMode)},
                    a.working_dir           as {nameof(AliasQueryResult.WorkingDirectory)},
                    a.icon                  as {nameof(AliasQueryResult.Icon)},
                    a.thumbnail             as {nameof(AliasQueryResult.Thumbnail)},
                    a.lua_script            as {nameof(AliasQueryResult.LuaScript)},
                    a.exec_count            as {nameof(AliasQueryResult.Count)},
                    a.hidden                as {nameof(AliasQueryResult.IsHidden)},
                    a.confirmation_required as {nameof(AliasQueryResult.IsExecutionConfirmationRequired)}
                from
                    alias a
                    left join alias_name n on a.id = n.id_alias
                    inner join data_alias_synonyms_v s on s.id_alias = a.id
                where
                    n.Name = @name
                    and hidden in @hidden
                order by
                    a.exec_count desc,
                    n.name";

        var hidden = includeHidden ? new[] { 0, 1 } : 0.ToEnumerable();
        var arguments = new { name, hidden };

        return _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, arguments).FirstOrDefault());
    }

    /// <summary>
    /// Get all the alias that has an exact name in the specified list of names.
    /// . In case of multiple aliases with same name, the one with greater counter
    /// is selected.
    /// </summary>
    /// <param name="names">The list of names to find.</param>
    /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    public IEnumerable<AliasQueryResult> GetExact(IEnumerable<string> names, bool includeHidden = false)
    {
        const string sql = @$"
            select
                n.Name                  as {nameof(AliasQueryResult.Name)},
                a.Id                    as {nameof(AliasQueryResult.Id)},
                a.arguments             as {nameof(AliasQueryResult.Parameters)},
                a.file_name             as {nameof(AliasQueryResult.FileName)},
                a.notes                 as {nameof(AliasQueryResult.Notes)},
                a.run_as                as {nameof(AliasQueryResult.RunAs)},
                a.start_mode            as {nameof(AliasQueryResult.StartMode)},
                a.working_dir           as {nameof(AliasQueryResult.WorkingDirectory)},
                a.icon                  as {nameof(AliasQueryResult.Icon)},
                a.thumbnail             as {nameof(AliasQueryResult.Thumbnail)},
                a.lua_script            as {nameof(AliasQueryResult.LuaScript)},
                a.hidden                as {nameof(AliasQueryResult.IsHidden)},
                a.confirmation_required as {nameof(AliasQueryResult.IsExecutionConfirmationRequired)}
            from
                alias a
                left join alias_name n on a.id = n.id_alias
            where
                n.Name in @names
                and hidden in @hidden
            order by n.name";

        var hidden = includeHidden ? new[] { 0, 1 } : 0.ToEnumerable();
        var arguments = new { names, hidden };

        return _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, arguments).ToArray());
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
        if (alias == null) throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias.");

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

    public bool SelectNames(AliasQueryResult alias)
    {
        const string sql = @"
            select count(*)
            from
	            alias_name an
                inner join alias a on an.id_alias = a.id
            where lower(name) = @name";

        var count = _db.WithinTransaction(tx => tx.Connection.ExecuteScalar<int>(sql, new { name = alias.Name }));
        return count > 0;
    }

    public ExistingNameResponse SelectNames(string[] names)
    {
        const string sql = @"
                select an.name
                from
                	alias_name an
                	inner join alias a on a.id = an.id_alias
                where an.name in @names";

        var result = _db.WithinTransaction(
            tx => tx.Connection.Query<string>(sql, new { names }).ToArray()
        );

        return new(result);
    }

    public long Update(AliasQueryResult alias) => _db.WithinTransaction(
        tx =>
        {
            const string updateAliasSql = @"
                update alias
                set
                    arguments             = @parameters,
                    file_name             = @fileName,
                    notes                 = @notes,
                    run_as                = @runAs,
                    start_mode            = @startMode,
                    working_dir           = @WorkingDirectory,
                    icon                  = @Icon,
                    thumbnail             = @thumbnail,
                    lua_script            = @luaScript,
                    exec_count            = @count,
                    confirmation_required = @isExecutionConfirmationRequired
                where id = @id;";

            using var _ = _logger.BeginSingleScope("Sql", updateAliasSql);
            tx.Connection.Execute(
                updateAliasSql,
                new
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
                    alias.Count,
                    id = alias.Id,
                    alias.IsExecutionConfirmationRequired
                }
            );
            CreateAdditionalParameters(alias, tx);
            UpdateName(alias, tx);
            alias.SynonymsWhenLoaded = alias.Synonyms;
            return alias.Id;
        }
    );

    public IEnumerable<long> UpdateThumbnails(IEnumerable<AliasQueryResult> aliases)
    {
        const string sql = "update alias set thumbnail = @thumbnail where id = @id";
        var ids = new List<long>();
        _db.WithinTransaction(
            tx =>
            {
                foreach (var alias in aliases)
                {
                    tx.Connection.Execute(sql, new { alias.Thumbnail, alias.Id });
                    ids.Add(alias.Id);
                }
            }
        );
        return ids.ToArray();
    }

    #endregion Methods
}