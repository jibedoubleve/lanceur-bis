using System.Data;
using Dapper;

namespace Lanceur.Infra.SQLite.DataAccess;

internal static class DbConnectionScopeMixin
{
    #region Methods

    public static int ExecuteMany(this IDbConnectionManager db, string sql, params long[] ids) => db.WithinTransaction(
        tx => ids.Sum(
            id => tx.Connection!
                    .Execute(sql, new { id })
        )
    );

    public static int ExecuteMany(this IDbConnection connection, string sql, params long[] ids)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ids.Sum(id => connection.Execute(sql, new { id }));
    }

    #endregion
}