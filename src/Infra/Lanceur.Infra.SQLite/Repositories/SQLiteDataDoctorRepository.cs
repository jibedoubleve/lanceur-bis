using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteDataDoctorRepository : SQLiteRepositoryBase, IDataDoctorRepository
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;

    #endregion

    #region Constructors

    public SQLiteDataDoctorRepository(IDbConnectionManager manager, ILoggerFactory loggerFactory, IDbActionFactory dbActionFactory) : base(manager) => _dbActionFactory = dbActionFactory;

    #endregion

    #region Methods

    private static void Update(IEnumerable<AliasQueryResult> aliases, IDbTransaction tx)
    {
        const string sql = """
                           update alias
                           set
                               icon = @icon
                           where
                               id = @id
                           """;
        foreach (var alias in aliases) tx.Connection!.Execute(sql, new { id = alias.Id, icon = alias.Icon });
    }

    public Task FixIconsForHyperlinksAsync() => Db.WithinTransaction(
        tx =>
        {
            {
                var aliases = _dbActionFactory.AliasSearchDbAction()
                                              .Search(tx)
                                              .ToArray();
                Update(aliases, tx);
                return Task.CompletedTask;
            }
        }
    );

    #endregion
}