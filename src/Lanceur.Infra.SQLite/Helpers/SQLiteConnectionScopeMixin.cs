using Dapper;
using System.Data;

namespace Lanceur.Infra.SQLite.Helpers;

internal static class SQLiteConnectionScopeMixin
{
    #region Methods

    public static int DeleteMany(this IDbConnectionManager db, string sql, params long[] ids)
        => db.WithinTransaction(tx => ids.Sum(id => tx.Connection
                                                      .Execute(sql, new { id })));

    #endregion Methods
}