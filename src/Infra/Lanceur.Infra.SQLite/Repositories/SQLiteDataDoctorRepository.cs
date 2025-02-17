using Dapper;
using Lanceur.Core.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteDataDoctorRepository : SQLiteRepositoryBase, IDataDoctorRepository
{
    #region Fields

    private readonly ILogger<SQLiteDataDoctorRepository> _logger;

    #endregion

    #region Constructors

    public SQLiteDataDoctorRepository(IDbConnectionManager manager, ILoggerFactory loggerFactory) : base(manager) => _logger = loggerFactory.CreateLogger<SQLiteDataDoctorRepository>();

    #endregion

    #region Methods

    public void ClearThumbnails() => Db.WithinTransaction(
        tx =>
        {
            {
                const string sql = "update alias set thumbnail = null";
                _logger.LogTrace("Clear thumbnails of all aliases");
                tx.Connection!.Execute(sql);
            }
        }
    );

    #endregion
}