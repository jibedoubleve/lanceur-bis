using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class SetUsageDbAction
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILoggerFactory _logFactory;

    private readonly ILogger<SetUsageDbAction> _logger;

    #endregion

    #region Constructors

    internal SetUsageDbAction(ILoggerFactory logFactory, IDbActionFactory dbActionFactory)
    {
        _logFactory = logFactory;
        _dbActionFactory = dbActionFactory;
        _logger = logFactory.GetLogger<SetUsageDbAction>();
    }

    #endregion

    #region Methods

    private void AddHistory(IDbTransaction tx,ref QueryResult alias)
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

    private void IncrementCounter(IDbTransaction tx, QueryResult alias)
    {
        alias.Count++;
        const string sql = """
                           update alias 
                           set 
                               exec_count = @counter 
                           where 
                               id = @id
                           """;
        var param = new { id = alias.Id, counter = alias.Count };
        tx.Connection!.Execute(sql, param);
    }

    internal void SetUsage(IDbTransaction tx, ref QueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);

        // When counter is set to -1, it means
        //   * usage should neither be maintained nor saved in history 
        //   * counter shouldn't be visible to the user
        if (alias.Count < 0) return;

        var aliasDbAction = _dbActionFactory.AliasManagement;
        if (alias.Id  == 0)
        {
            if (aliasDbAction.GetExact(alias?.Name, tx) is { } a)
                alias!.Id = a.Id;
            else
                aliasDbAction.CreateInvisible(tx, ref alias);
        }

        AddHistory(tx, ref alias);
        IncrementCounter(tx, alias);
    }

    #endregion
}