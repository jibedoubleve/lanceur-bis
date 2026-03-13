using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite.Repositories;

/// <inheritdoc cref="IFeatureFlagRepository" />
public class SQLiteFeatureFlagRepository : SQLiteRepositoryBase, IFeatureFlagRepository
{
    #region Constructors

    public SQLiteFeatureFlagRepository(IDbConnectionManager manager) : base(manager) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<FeatureFlag> GetFeatureFlags()
    {
        const string sql = """
                           select s_value ->> '$.FeatureFlags'
                           from settings
                           where s_key = 'json'
                           """;
        var json = Db.WithConnection(conn => conn.Query<string>(sql).SingleOrDefault());

        return json.IsNullOrEmpty()
            ? new ApplicationSettings().FeatureFlags
            : JsonConvert.DeserializeObject<IEnumerable<FeatureFlag>>(json!) ?? [];
    }

    #endregion
}