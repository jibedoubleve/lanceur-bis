using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Sqlite.Entities;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

//TODO: reorder the IDbTransaction it should be the first argument of any method
public class AliasDbAction
{
    #region Fields

    private readonly ILogger<AliasDbAction> _logger;

    #endregion

    #region Constructors

    internal AliasDbAction(ILoggerFactory logFactory)  => _logger = logFactory.GetLogger<AliasDbAction>();

    #endregion

    #region Methods

    private void ClearAlias(IDbTransaction tx, params long[] ids)
    {
        const string sql = """
                           delete from alias
                           where id = @id
                           """;

        var cnt = tx.Connection.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias. Id: {Idd}", cnt, idd);
    }

    private void ClearAliasArgument(IDbTransaction tx, params long[] ids)
    {
        const string sql = """
                           delete from alias_argument
                           where id_alias = @id
                           """;

        var cnt = tx.Connection.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias_argument. Id: {IdAliases}", cnt, idd);
    }

    private void ClearAliasName(IDbTransaction tx, params long[] ids)
    {
        const string sql = """
                           delete from alias_name
                           where id_alias = @id
                           """;

        var cnt = tx.Connection.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {RemovedCount} row(s) from alias_name. Id: {IdAliases}", cnt, idd);
    }

    private void ClearAliasUsage(IDbTransaction tx, params long[] ids)
    {
        const string sql = """
                           delete from alias_usage
                           where id_alias = @id;
                           """;

        var cnt = tx.Connection.ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogInformation("Removed {Count} row(s) from alias_usage. Id: {IdAliases}", cnt, idd);
    }

    private void CreateAdditionalParameters(IDbTransaction tx, long idAlias, IEnumerable<AdditionalParameter> parameters)
    {
        using var _ = _logger.BeginSingleScope("Parameters", parameters);
        const string sql1 = "delete from alias_argument where id_alias = @idAlias";
        const string sql2 = "insert into alias_argument (id_alias, argument, name) values(@idAlias, @parameter, @name);";

        // Remove existing additional alias parameters
        var deletedRowsCount = tx.Connection!.Execute(sql1, new { idAlias });

        // Create alias additional parameters
        var addedRowsCount = tx.Connection.Execute(sql2, parameters.ToEntity(idAlias));

        if (deletedRowsCount > 0 && addedRowsCount == 0) _logger.LogWarning("Deleting {DeletedRowsCount} parameters while adding no new parameters", deletedRowsCount);
    }

    private void CreateAdditionalParameters(IDbTransaction tx, AliasQueryResult alias) => CreateAdditionalParameters(tx, alias.Id, alias.AdditionalParameters);

    private static void UpdateName(IDbTransaction tx, AliasQueryResult alias)
    {
        //Remove all names
        const string sql = @"delete from alias_name where id_alias = @id";
        tx.Connection!.Execute(sql, new { id = alias.Id });

        //Recreate new names
        const string sql2 = @"insert into alias_name (id_alias, name) values (@id, @name)";
        foreach (var name in alias.Synonyms.SplitCsv()) tx.Connection.Execute(sql2, new { id = alias.Id, name });
    }

    internal long Create(IDbTransaction tx, ref AliasQueryResult alias)
    {
        const string sqlAlias = """

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
                                select last_insert_rowid() from alias limit 1;
                                """;

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
        var id = tx.Connection!.ExecuteScalar<long>(sqlAlias, param);

        // Create synonyms
        const string sqlSynonyms = @"insert into alias_name (id_alias, name) values (@id, @name)";
        foreach (var name in csv) tx.Connection.ExecuteScalar<long>(sqlSynonyms, new { id, name });

        //Create additional arguments
        CreateAdditionalParameters(tx, id, additionalParameters);

        alias.Id = id;

        // Return either the id of the created Alias or
        // return the id of the just updated alias (which
        // should be the same as the one specified as arg)
        return id;
    }

    internal void CreateInvisible(IDbTransaction tx, ref QueryResult alias)
    {
        var queryResult = new AliasQueryResult { Name = alias.Name, FileName = alias.Name, Synonyms = alias.Name, IsHidden = true };
        alias.Id = Create(tx, ref queryResult);
    }

    internal AliasQueryResult GetByIdAndName(long id, string name, IDbTransaction tx)
    {
        if (id <= 0) throw new ArgumentException("The id of the alias should be greater than zero.");

        const string sql = $"""
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
                                and a.Id = @id
                            order by
                                a.exec_count desc,
                                n.name
                            """;

        return tx.Connection!
                 .Query<AliasQueryResult>(sql, new { id, name })
                 .SingleOrDefault();
    }

    /// <summary>
    ///     Get the first alias with the exact name.
    /// </summary>
    /// <param name="name">The alias' exact name to find.</param>
    /// <param name="tx">Transaction to use for the query</param>
    /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    /// <remarks>
    ///     For optimisation reason, there's no check of doubloons. Bear UI validates and
    ///     forbid to insert two aliases with same name.
    /// </remarks>
    internal AliasQueryResult GetExact(string name, IDbTransaction tx, bool includeHidden = false)
    {
        const string sql = $"""
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
                                n.name
                            """;

        var hidden = includeHidden ? [0, 1] : 0.ToEnumerable();
        var arguments = new { name, hidden };

        return tx.Connection!.Query<AliasQueryResult>(sql, arguments).FirstOrDefault();
    }

    /// <summary>
    ///     Get all the alias that has an exact name in the specified list of names.
    ///     . In case of multiple aliases with same name, the one with greater counter
    ///     is selected.
    /// </summary>
    /// <param name="names">The list of names to find.</param>
    /// <param name="tx">The transaction to use for the query</param>
    /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    internal IEnumerable<AliasQueryResult> GetExact(IEnumerable<string> names, IDbTransaction tx, bool includeHidden = false)
    {
        const string sql = $"""
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
                            order by n.name
                            """;

        var hidden = includeHidden ? [0, 1] : 0.ToEnumerable();
        var arguments = new { names, hidden };

        return tx.Connection!.Query<AliasQueryResult>(sql, arguments).ToArray();
    }

    internal KeywordUsage GetHiddenKeyword(string name, IDbTransaction tx)
    {
        const string sql = """
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
                               and n.name = @name;
                           """;
        var result = tx.Connection!.Query<KeywordUsage>(sql, new { name }).FirstOrDefault();

        return result;
    }

    internal bool HasNames(AliasQueryResult alias, IDbTransaction tx)
    {
        const string sql = """
                           select count(*)
                           from
                           	alias_name an
                               inner join alias a on an.id_alias = a.id
                           where lower(name) = @name
                           """;

        var count = tx.Connection!.ExecuteScalar<int>(sql, new { name = alias.Name });
        return count > 0;
    }

    internal IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> results, IDbTransaction tx)
    {
        const string sql = $"""

                            select
                            	id_alias as {nameof(KeywordUsage.Id)},
                            	count
                            from
                            	stat_usage_per_app_v
                            where id_alias in @ids;
                            """;

        var dbResultAr = results as QueryResult[] ?? results.ToArray();
        var dbResults = tx.Connection!.Query<KeywordUsage>(sql, new { ids = dbResultAr.Select(x => x.Id).ToArray() })
                          .ToArray();

        foreach (var result in dbResultAr)
            result.Count = dbResults.Where(item => item.Id == result.Id)
                                    .Select(item => item.Count)
                                    .SingleOrDefault();
        return dbResultAr;
    }

    internal void Remove(AliasQueryResult alias, IDbTransaction tx)
    {
        if (alias == null) throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias.");

        ClearAliasUsage(tx, alias.Id);
        ClearAliasArgument(tx, alias.Id);
        ClearAliasName(tx, alias.Id);
        ClearAlias(tx, alias.Id);
    }

    internal void RemoveMany(IEnumerable<AliasQueryResult> alias, IDbTransaction tx)
    {
        ArgumentNullException.ThrowIfNull(alias);
        var ids = alias.Select(x => x.Id).ToArray();

        ClearAliasUsage(tx, ids);
        ClearAliasArgument(tx, ids);
        ClearAliasName(tx, ids);
        ClearAlias(tx, ids);
    }

    internal ExistingNameResponse SelectNames(string[] names, IDbTransaction tx)
    {
        const string sql = """
                           select an.name
                           from
                               alias_name an
                               inner join alias a on a.id = an.id_alias
                           where an.name in @names
                           """;

        var result = tx.Connection!.Query<string>(sql, new { names }).ToArray();

        return new(result);
    }

    internal long Update(AliasQueryResult alias, IDbTransaction tx)
    {
        const string updateAliasSql = """
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
                                      where id = @id;
                                      """;

        using var _ = _logger.BeginSingleScope("Sql", updateAliasSql);
        tx.Connection!.Execute(
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
        CreateAdditionalParameters(tx, alias);
        UpdateName(tx, alias);
        alias.SynonymsWhenLoaded = alias.Synonyms;
        return alias.Id;
    }

    internal IEnumerable<long> UpdateThumbnails(IEnumerable<AliasQueryResult> aliases, IDbTransaction tx)
    {
        const string sql = "update alias set thumbnail = @thumbnail where id = @id";
        var ids = new List<long>();
        foreach (var alias in aliases)
        {
            tx.Connection!.Execute(sql, new { alias.Thumbnail, alias.Id });
            ids.Add(alias.Id);
        }

        return ids.ToArray();
    }

    #endregion
}