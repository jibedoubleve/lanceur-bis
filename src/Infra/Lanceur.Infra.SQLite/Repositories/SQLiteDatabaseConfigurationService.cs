using Dapper;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite.Repositories;

/// <inheritdoc cref="IDatabaseConfigurationService"/>
public class SQLiteDatabaseConfigurationService : SQLiteRepositoryBase, IDatabaseConfigurationService
{
    #region Fields

    private DatabaseConfiguration _current;

    #endregion Fields

    #region Constructors

    public SQLiteDatabaseConfigurationService(IDbConnectionManager manager) : base(manager) { }

    #endregion Constructors

    #region Properties

    public DatabaseConfiguration Current
    {
        get
        {
            if (_current is null) Load();
            return _current;
        }
    }

    #endregion Properties

    #region Methods

    public void Edit(Action<DatabaseConfiguration> edit)
    {
        var stg = Current;
        edit(stg);
        Save();
    }

    public void Load()
    {
        const string sql = """
                           select
                               s_value as Value
                           from settings
                           where s_key = 'json';
                           """;
        var s = Db.WithinTransaction(
            tx =>
                tx.Connection!.Query<string>(sql)
                  .FirstOrDefault()
        );

        _current = s.IsNullOrEmpty()
            ? new()
            : JsonConvert.DeserializeObject<DatabaseConfiguration>(s);
    }

    public void Save()
    {
        Db.WithinTransaction(
            tx =>
            {
                const string sql = """
                                   insert into settings(s_key, s_value) values ('json', @json)
                                   on conflict (s_key) do update 
                                   set 
                                   	s_value = @json
                                   where s_key = 'json'
                                   """;
                var json = JsonConvert.SerializeObject(Current);
                tx.Connection!.Execute(sql, new { json });
            }
        );
    }

    #endregion Methods
}