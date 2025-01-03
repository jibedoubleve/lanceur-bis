using System.Data.Common;

namespace Lanceur.Infra.SQLite.DataAccess;

public interface IDbConnectionFactory
{
    #region Methods

    DbConnection CreateConnection();

    #endregion
}