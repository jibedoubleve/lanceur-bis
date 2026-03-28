using System.Data;
using Dapper;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Sqlite.Entities;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public sealed class AliasDbAction
{
    #region Fields

    private readonly ILogger<AliasDbAction> _logger;

    private const string MessageRemovedRows = "Removed {Count} row(s) from {Table}. Id: {IdAliases}";

    #endregion

    #region Constructors

    internal AliasDbAction(ILoggerFactory logFactory) => _logger = logFactory.GetLogger<AliasDbAction>();

    #endregion

    #region Methods

    private void ClearAlias(IDbTransaction tx, params long[] ids)
    {
        const string sql = "delete from alias where id = @id";

        var cnt = tx.GetConnection().ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogDebug(
            MessageRemovedRows,
            cnt,
            "alias",
            idd
        );
    }

    private void ClearAliasArgument(IDbTransaction tx, params long[] ids)
    {
        const string sql = "delete from alias_argument where id_alias = @id";

        var cnt = tx.GetConnection().ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogDebug(
            MessageRemovedRows,
            cnt,
            "alias_argument",
            idd
        );
    }

    private void ClearAliasName(IDbTransaction tx, params long[] ids)
    {
        const string sql = "delete from alias_name where id_alias = @id";

        var cnt = tx.GetConnection().ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogDebug(
            MessageRemovedRows,
            cnt,
            "alias_name",
            idd
        );
    }

    private void ClearAliasUsage(IDbTransaction tx, params long[] ids)
    {
        const string sql = "delete from alias_usage where id_alias = @id;";

        var cnt = tx.GetConnection().ExecuteMany(sql, ids);
        var idd = string.Join(", ", ids);
        _logger.LogDebug(
            MessageRemovedRows,
            cnt,
            "alias_usage",
            idd
        );
    }

    private void CreateAdditionalParameters(
        IDbTransaction tx,
        long idAlias,
        IEnumerable<AdditionalParameter> parameters
    )
    {
        parameters = parameters.ToList();
        using var _ = _logger.BeginSingleScope("Parameters", parameters);
        const string sql1 = "delete from alias_argument where id_alias = @idAlias";
        const string sql2
            = "insert into alias_argument (id_alias, argument, name) values(@idAlias, @parameter, @name);";

        // Remove existing additional alias parameters
        var deletedRowsCount = tx.Connection!.Execute(sql1, new { idAlias });

        // Create alias additional parameters
        var entities = parameters.ToEntity(idAlias).ToList();
        var addedRowsCount
            = entities.Sum(entity =>
                tx.GetConnection().Execute(
                    sql2,
                    new { idAlias = entity.IdAlias, parameter = entity.Parameter, name = entity.Name }
                )
            );

        if (deletedRowsCount > 0 && addedRowsCount == 0)
        {
            _logger.LogWarning(
                "Deleting {DeletedRowsCount} parameters while adding no new parameters",
                deletedRowsCount
            );
        }
    }

    private static string? GetFileName(QueryResult alias) =>
        alias is AliasQueryResult aqr
            ? aqr.FileName
            : null;

    internal void CreateInvisible(IDbTransaction tx, QueryResult alias)
    {
        if (alias is not ExecutableQueryResult exec) { return; }

        var queryResult = exec.ToAliasQueryResult();
        queryResult.IsHidden = true;
        queryResult.FileName = GetFileName(alias) ?? exec.Name; // By convention for builtin keyword
        alias.Id = SaveOrUpdate(tx, queryResult);
    }

    internal AliasQueryResult? GetById(IDbConnection connection, long id)
    {
        if (id <= 0) { throw new ArgumentException("The id of the alias should be greater than zero."); }

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
                                a.thumbnail             as {nameof(AliasQueryResult.Thumbnail)},
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

        var result = connection.Query<AliasQueryResult>(sql, new { id })
                               .SingleOrDefault();
        if (result is null) { _logger.LogTrace("No alias found for id {Id}", id); }

        return result;
    }

    /// <summary>
    ///     Returns all aliases whose name exactly matches one of the specified names.
    /// </summary>
    /// <param name="connection">The database connection to use for the query.</param>
    /// <param name="names">The list of names to search for.</param>
    /// <param name="includeHidden">Whether to include hidden aliases in the results.</param>
    /// <returns>All matching aliases, or an empty collection if none are found.</returns>
    internal IEnumerable<AliasQueryResult> GetByNames(
        IDbConnection connection,
        IEnumerable<string> names,
        bool includeHidden = false
    )
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

        var result = connection.Query<AliasQueryResult>(sql, arguments).ToArray();

        if (result.Length == 0) { _logger.LogTrace("No alias found with names {Names}", names); }

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
        foreach (var alias in aliases)
        {
            LogicalRemove(tx, alias);
        }
    }

    /// <summary>
    ///     Permanently deletes the specified alias and all its associated data from the database,
    ///     including its usage history, additional parameters, and synonyms.
    ///     This operation is irreversible and cannot be undone.
    /// </summary>
    /// <param name="tx">The database transaction used to execute the deletion.</param>
    /// <param name="id">The primary key of the alias to permanently delete.</param>
    internal void Remove(IDbTransaction tx, long id)
    {
        ClearAliasUsage(tx, id);
        ClearAliasArgument(tx, id);
        ClearAliasName(tx, id);
        ClearAlias(tx, id);
    }

    internal long SaveOrUpdate(IDbTransaction tx, AliasQueryResult alias)
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
                                    thumbnail   = @thumbnail,
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
                                    thumbnail,
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
                                    @thumbnail,
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
            thumbnail = alias.Thumbnail,
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
        tx.GetConnection().Execute(sqlDelete, new { id });

        // Add the updated synonyms 
        var names = alias.Synonyms ?? alias.Name ?? throw new ArgumentException("The alias to create has no name");

        var csv = names.SplitCsv();
        const string sqlSynonyms = "insert into alias_name (id_alias, name) values (@id, @name)";
        foreach (var name in csv)
        {
            tx.GetConnection().ExecuteScalar<long>(sqlSynonyms, new { id, name });
        }

        // Additional parameters
        var additionalParameters = alias.AdditionalParameters;
        CreateAdditionalParameters(tx, id, additionalParameters);

        // Return either the id of the created Alias or
        // return the id of the just updated alias (which
        // should be the same as the one specified as arg)
        return alias.Id;
    }

    /// <summary>
    ///     Tries to find the alias id based on the <c>file_name</c>.
    ///     If the query returns more than one result, or zero result, the method fails by returning <c>false</c>
    ///     and sets <paramref name="id" /> to <c>0</c>.
    /// </summary>
    /// <param name="conn">The database connection used to execute the query.</param>
    /// <param name="alias">The alias whose <c>file_name</c> is used to search for the id.</param>
    /// <param name="id">
    ///     When this method returns <c>true</c>, contains the id of the matching alias;
    ///     otherwise, <c>0</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if exactly one alias was found; <c>false</c> if none or more than one were found,
    ///     or if the alias has no <c>file_name</c>.
    /// </returns>
    internal bool TryFindId(IDbConnection conn, QueryResult alias, out long id)
    {
        const string sql = "select id from alias where file_name = @fileName;";
        var fileName = GetFileName(alias);

        if (fileName.IsNullOrEmpty())
        {
            id = 0;
            return false;
        }

        var result = conn.Query<long>(sql, new { fileName }).ToList();

        switch (result.Count)
        {
            case 0:
                id = 0;
                return false;
            case 1:
                id = result[0];
                return true;
            default:
                _logger.LogWarning("Doubloons detected for alias with file name '{FileName}'", fileName);
                id = 0;
                return false;
        }
    }

    #endregion
}