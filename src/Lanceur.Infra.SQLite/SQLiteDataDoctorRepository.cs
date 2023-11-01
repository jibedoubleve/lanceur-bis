using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DbActions;

namespace Lanceur.Infra.SQLite;

public class SQLiteDataDoctorRepository : SQLiteRepositoryBase, IDataDoctorRepository
{
    #region Fields

    private readonly GetAllAliasDbAction _dbAction;

    #endregion Fields

    #region Constructors

    public SQLiteDataDoctorRepository(
        IDbConnectionManager manager,
        IAppLoggerFactory logFactory) : base(manager) => _dbAction = new(DB, logFactory);

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
        DB.WithinTransaction(tx =>
        {
            foreach (var alias in aliases) tx.Connection.Execute(sql, new { id = alias.Id, icon = alias.Icon });
        });
    }

    public Task FixIconsForHyperlinksAsync()
    {
        var aliases = _dbAction.GetAll()
                               .ToArray();

        foreach (var alias in aliases) alias.UpdateIconForHyperlinks();

        Update(aliases);
        return Task.CompletedTask;
    }

    #endregion Methods
}