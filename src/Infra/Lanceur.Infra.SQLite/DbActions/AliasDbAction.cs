using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Sqlite.Entities;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

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
        parameters = parameters.ToList();
        using var _ = _logger.BeginSingleScope("Parameters", parameters);
        const string sql1 = "delete from alias_argument where id_alias = @idAlias";
        const string sql2 = "insert into alias_argument (id_alias, argument, name) values(@idAlias, @parameter, @name);";

        // Remove existing additional alias parameters
        var deletedRowsCount = tx.Connection!.Execute(sql1, new { idAlias });

        // Create alias additional parameters
        var entities = parameters.ToEntity(idAlias).ToList();
        var addedRowsCount = entities.Sum(
            entity =>
                tx.Connection.Execute(sql2, new { idAlias = entity.IdAlias, parameter = entity.Parameter, name = entity.Name })
        );

        if (deletedRowsCount > 0 && addedRowsCount == 0) _logger.LogWarning("Deleting {DeletedRowsCount} parameters while adding no new parameters", deletedRowsCount);
    }

    internal void CreateInvisible(IDbTransaction tx, ref QueryResult alias)
    {
        if (alias is not AliasQueryResult queryResult) return;

        queryResult.IsHidden = true;
        alias.Id = SaveOrUpdate(tx, ref queryResult);
    }

    internal AliasQueryResult GetById(IDbTransaction tx, long id)
    {
        if (id <= 0) throw new ArgumentException("The id of the alias should be greater than zero.");

        const string sql = $"""
                            select
                                n.Name                  as {nameof(AliasQueryResult.Name)},
                                s.synonyms              as {nameof(AliasQueryResult.Synonyms)},
                                a.Id                    as {nameof(AliasQueryResult.Id)},
                                a.arguments             as {nameof(AliasQueryResult.Parameters)},
                                a.file_name             as {nameof(AliasQueryResult.FileName)},
                                a.notes                 as {nameof(AliasQueryResult.Description)},
                                a.run_as                as {nameof(AliasQueryResult.RunAs)},
                                a.start_mode            as {nameof(AliasQueryResult.StartMode)},
                                a.working_dir           as {nameof(AliasQueryResult.WorkingDirectory)},
                                a.icon                  as {nameof(AliasQueryResult.Icon)},
                                a.lua_script            as {nameof(AliasQueryResult.LuaScript)},
                                a.exec_count            as {nameof(AliasQueryResult.Count)},
                                a.hidden                as {nameof(AliasQueryResult.IsHidden)},
                                a.confirmation_required as {nameof(AliasQueryResult.IsExecutionConfirmationRequired)}
                            from
                                alias a
                                left join alias_name n on a.id = n.id_alias
                                left join data_alias_synonyms_v s on s.id_alias = a.id
                            where
                                a.Id = @id
                            order by
                                a.exec_count desc,
                                n.name
                            limit 1;
                            """;

        var result = tx.Connection!
                       .Query<AliasQueryResult>(sql, new { id })
                       .SingleOrDefault();

        return result;
    }

    /// <summary>
    ///     Get all the alias that has an exact name in the specified list of names.
    ///     . In case of multiple aliases with same name, the one with greater counter
    ///     is selected.
    /// </summary>
    /// <param name="tx">The transaction to use for the query</param>
    /// <param name="names">The list of names to find.</param>
    /// <param name="includeHidden">Indicate whether include or not hidden aliases</param>
    /// <returns>The exact match or <c>null</c> if not found.</returns>
    internal IEnumerable<AliasQueryResult> GetByNames(IDbTransaction tx, IEnumerable<string> names, bool includeHidden = false)
    {
        const string sql = $"""
                            select
                                n.Name                  as {nameof(AliasQueryResult.Name)},
                                a.Id                    as {nameof(AliasQueryResult.Id)},
                                a.arguments             as {nameof(AliasQueryResult.Parameters)},
                                a.file_name             as {nameof(AliasQueryResult.FileName)},
                                a.notes                 as {nameof(AliasQueryResult.Description)},
                                a.run_as                as {nameof(AliasQueryResult.RunAs)},
                                a.start_mode            as {nameof(AliasQueryResult.StartMode)},
                                a.working_dir           as {nameof(AliasQueryResult.WorkingDirectory)},
                                a.icon                  as {nameof(AliasQueryResult.Icon)},
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

    internal AliasUsage GetHiddenAlias(IDbTransaction tx, string name)
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
        var result = tx.Connection!.Query<AliasUsage>(sql, new { name }).FirstOrDefault();

        return result;
    }

    internal void LogicalRemove(IDbTransaction tx, AliasQueryResult alias)
    {
        const string sql = """
                           update alias 
                           set 
                               deleted_at = @deleted_at,
                               hidden     = 1
                           where id = @id;
                           """;
        tx.Connection!.Execute(sql, new { deleted_at = DateTime.Now, id = alias.Id });
    }

    internal void LogicalRemove(IDbTransaction tx, IEnumerable<AliasQueryResult> aliases)
    {
        foreach (var alias in aliases) LogicalRemove(tx, alias);
    }

    /// <summary>
    ///     Permanently deletes the specified alias and all associated information from the database.
    ///     This operation is irreversible and cannot be undone.
    /// </summary>
    /// <param name="tx">The database transaction used to execute the deletion operation.</param>
    /// <param name="alias">The alias to be removed, encapsulated in an <see cref="AliasQueryResult" /> object.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="tx" /> or <paramref name="alias" /> parameter is <c>null</c>.
    /// </exception>
    internal void Remove(IDbTransaction tx, AliasQueryResult alias)
    {
        if (alias == null) throw new ArgumentNullException(nameof(alias), "Cannot delete NULL alias.");

        ClearAliasUsage(tx, alias.Id);
        ClearAliasArgument(tx, alias.Id);
        ClearAliasName(tx, alias.Id);
        ClearAlias(tx, alias.Id);
    }

    internal long SaveOrUpdate(IDbTransaction tx, ref AliasQueryResult alias)
    {
        const string sqlAlias = """
                                update alias
                                set
                                    arguments   = @arguments,
                                    file_name   = @fileName,
                                    notes       = @notes,
                                    run_as      = @runAs,
                                    start_mode  = @startMode,
                                    working_dir = @workingDirectory,
                                    icon        = @icon,
                                    lua_script  = @luaScript,
                                    hidden      = @isHidden,
                                    confirmation_required = @isExecutionConfirmationRequired
                                where id = @id;

                                insert into alias (
                                    id,
                                    arguments,
                                    file_name,
                                    notes,
                                    run_as,
                                    start_mode,
                                    working_dir,
                                    icon,
                                    lua_script,
                                    hidden,
                                    confirmation_required
                                ) values (
                                    @id,
                                    @arguments,
                                    @fileName,
                                    @notes,
                                    @runAs,
                                    @startMode,
                                    @workingDirectory,
                                    @icon,
                                    @luaScript,
                                    @isHidden,
                                    @isExecutionConfirmationRequired
                                )
                                on conflict (id) do nothing
                                returning id;
                                """;

        var param = new
        {
            arguments = alias.Parameters,
            id = alias.Id == 0 ? null : (int?)alias.Id,
            fileName = alias.FileName,
            notes = alias.Description,
            runAs = alias.RunAs,
            startMode = alias.StartMode,
            workingDirectory = alias.WorkingDirectory,
            icon = alias.Icon,
            luaScript = alias.LuaScript,
            isHidden = alias.IsHidden,
            isExecutionConfirmationRequired = alias.IsExecutionConfirmationRequired
        };

        var id = tx.Connection!.ExecuteScalar<long>(sqlAlias, param);

        // If the 'id' is 0 (indicating that an UPDATE was performed instead of an INSERT),
        // reassign the 'id'. This ensures the correct ID is used for subsequent operations.
        id = id == 0 ? alias.Id : id;
        alias.Id = id;

        // Remove 
        const string sqlDelete = "delete from alias_name where id_alias = @id;";
        tx.Connection.Execute(sqlDelete, new { id });

        // Add the updated synonyms 
        var csv = alias.Synonyms.SplitCsv();
        const string sqlSynonyms = "insert into alias_name (id_alias, name) values (@id, @name)";
        foreach (var name in csv) tx.Connection.ExecuteScalar<long>(sqlSynonyms, new { id, name });

        // Additional parameters
        var additionalParameters = alias.AdditionalParameters;
        CreateAdditionalParameters(tx, id, additionalParameters);

        // Return either the id of the created Alias or
        // return the id of the just updated alias (which
        // should be the same as the one specified as arg)
        return alias.Id;
    }

    internal ExistingNameResponse SelectNames(IDbTransaction tx, string[] names)
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


    /// <summary>
    ///     Attempts to find the specified alias in the database. If found, updates the alias's ID and returns <c>true</c>.
    ///     If not found, returns <c>false</c> and leaves the alias unchanged.
    ///     The method performs a database query to find aliases matching the following criteria:
    ///     - Same <c>file_name</c>
    ///     - Alias is marked as hidden (<c>hidden = true</c>)
    ///     If multiple aliases are found, the method will return <c>false</c>.
    /// </summary>
    /// <param name="tx">The database transaction to be used for querying.</param>
    /// <param name="alias">The alias to search for. If found, its ID will be updated.</param>
    /// <returns><c>true</c> if the alias was found and its ID was updated; otherwise, <c>false</c>.</returns>
    internal bool TryFind(IDbTransaction tx, ref AliasQueryResult alias)
    {
        const string sql = """
                           select
                               Id 
                           from
                               alias 
                           where
                               file_name = @fileName
                               and hidden = true
                           order by
                               exec_count desc
                           """;
        var foundAlias = tx.Connection!.Query<AliasQueryResult>(
                               sql,
                               new { fileName = alias.FileName }
                           )
                           .SingleOrDefault();

        if (foundAlias is not null) alias.Id = foundAlias.Id;
        return foundAlias is not null;
    }

    #endregion
}