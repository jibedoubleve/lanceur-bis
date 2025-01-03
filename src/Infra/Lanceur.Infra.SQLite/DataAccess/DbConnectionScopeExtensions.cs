using System.Data;
using Dapper;

namespace Lanceur.Infra.SQLite.DataAccess;

internal static class DbConnectionScopeExtensions
{
    #region Methods

    public static int ExecuteMany(this IDbConnection connection, string sql, params long[] ids)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return ids.Sum(id => connection.Execute(sql, new { id }));
    }

    #endregion
}