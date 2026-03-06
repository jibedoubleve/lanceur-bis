using System.Data;

namespace Lanceur.Infra.SQLite.Extensions;

public static class TransactionExtension
{
    internal static IDbConnection GetConnection(this IDbTransaction tx)
    {
        if (tx.Connection is null)
        {
            throw new InvalidOperationException(
                "Cannot set the current version ot the application because no connection exists");
        }
        return tx.Connection;
    }
}