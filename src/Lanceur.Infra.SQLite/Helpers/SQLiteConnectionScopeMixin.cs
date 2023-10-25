using Dapper;
using System.Data;

namespace Lanceur.Infra.SQLite.Helpers;

internal static class SQLiteConnectionScopeMixin
{
    #region Methods

    public static int DeleteMany(this SQLiteConnectionScope db, string sql, params long[] ids)
    {
        var cnt = 0;
        if (db.Connection.State != ConnectionState.Open)
        {
            db.Connection.Open();
        }

        using var scope = db.Connection.BeginTransaction();
        cnt = ids.Sum(id => db.Connection.Execute(sql, new { id }));
        scope.Commit();
        return cnt;
    }

    #endregion Methods
}