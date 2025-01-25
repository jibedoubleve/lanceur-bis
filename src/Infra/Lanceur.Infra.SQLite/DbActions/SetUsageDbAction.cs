using System.Data;
using Dapper;
using Lanceur.Core.Models;

namespace Lanceur.Infra.SQLite.DbActions;

public class SetUsageDbAction
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;

    #endregion

    #region Constructors

    internal SetUsageDbAction(IDbActionFactory dbActionFactory) => _dbActionFactory = dbActionFactory;

    #endregion

    #region Methods

    private void AddHistory(IDbTransaction tx, ref QueryResult alias)
    {
        const string sql = """
                           insert into alias_usage (
                               id_alias,
                               time_stamp
                           ) values (
                               @idAlias,
                               @now
                           )
                           """;

        var param = new { idAlias = alias.Id, now = DateTime.Now };
        tx.Connection!.Execute(sql, param);
    }

    /// <summary>
    ///     Retrieves the usage count for a given alias from the usage table and updates the count property of the alias
    ///     object.
    /// </summary>
    /// <remarks>
    ///     This method is designed for optimisation and should be executed in a fire-and-forget manner.
    ///     A slight delay in updating the count is acceptable and preferable compared to adding latency to each search
    ///     operation.
    ///     This is particularly beneficial in scenarios where numerous search queries are executed while the user is typing
    ///     to locate the desired alias.
    /// </remarks>
    /// <param name="tx">The database transaction context used to execute the query safely.</param>
    /// <param name="alias">
    ///     A reference to the QueryResult object whose count property will be updated with the retrieved
    ///     value.
    /// </param>
    private static void UpdateCounter(IDbTransaction tx, ref QueryResult alias)
    {
        const string sql = "select count(*) from alias_usage where id_alias = @id;";
        var count = tx.Connection!.ExecuteScalar<int>(sql, new { id = alias.Id });

        const string sqlUpdate = "update alias set exec_count = @count where id = @id;";
        tx.Connection!.Execute(sqlUpdate, new { id = alias.Id,   count });

        alias.Count = count;
    }

    /// <summary>
    /// Adds an entry to the usage table with the alias and the current date and time, 
    /// and updates the counter of the specified QueryResult.
    /// </summary>
    /// <remarks>
    /// This method has a side effect: it modifies the counter of the provided alias.  
    /// If the counter is negative, no usage is recorded or saved in the history, and the counter remains hidden from the user.
    /// </remarks>
    /// <param name="tx">The database transaction context.</param>
    /// <param name="alias">The QueryResult object representing the alias to be updated. Must not be null.</param>
    internal void SetUsage(IDbTransaction tx, ref QueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);

        // A negative counter (-1) indicates:
        //   * Usage tracking is disabled.
        //   * The alias is excluded from the history.
        //   * The counter is hidden from the user.
        if (alias.Count < 0) return;

        var aliasDbAction = _dbActionFactory.AliasManagement;
        if (alias.Id  == 0)
        {
            if (alias is AliasQueryResult aqr &&  aliasDbAction.TryFind(tx, ref aqr))
                alias!.Id = aqr.Id;
            else
                aliasDbAction.CreateInvisible(tx, ref alias);
        }

        AddHistory(tx, ref alias);
        UpdateCounter(tx, ref alias);
    }

    #endregion
}