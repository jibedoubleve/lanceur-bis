using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite;

public class SQLiteDataDoctorRepository : SQLiteRepositoryBase, IDataDoctorRepository
{
    #region Fields

    private readonly AliasSearchDbAction _dbAction;

    #endregion Fields

    #region Constructors

    public SQLiteDataDoctorRepository(IDbConnectionManager manager, ILoggerFactory loggerFactory, IMappingService converter) : base(manager) => _dbAction = new(DB, loggerFactory, converter);

    #endregion Constructors

    #region Methods

    private void Update(IEnumerable<AliasQueryResult> aliases)
    {
        const string sql = @"
                update alias
                set
                    icon = @icon
                where
                    id = @id";
        DB.WithinTransaction(
            tx =>
            {
                foreach (var alias in aliases) tx.Connection.Execute(sql, new { id = alias.Id, icon = alias.Icon });
            }
        );
    }

    public Task FixIconsForHyperlinksAsync()
    {
        var aliases = _dbAction.Search()
                               .ToArray();
        Update(aliases);
        return Task.CompletedTask;
    }

    #endregion Methods
}