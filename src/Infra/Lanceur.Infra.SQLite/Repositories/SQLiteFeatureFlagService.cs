using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite.Repositories;

/// <inheritdoc cref="IFeatureFlagService" />
public class SQLiteFeatureFlagService : SQLiteRepositoryBase, IFeatureFlagService
{
    #region Constructors

    public SQLiteFeatureFlagService(IDbConnectionManager manager) : base(manager) { }

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
        return Db.WithConnection(
            conn =>
            {
                var json = conn.Query<string>(sql).SingleOrDefault() ?? string.Empty;

                return json.IsNullOrEmpty() 
                    ? new ApplicationSettings().FeatureFlags 
                    : JsonConvert.DeserializeObject<IEnumerable<FeatureFlag>>(json);
            }
        );
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName) => GetFeatureFlags()
                                                 .Where(e => string.Equals(e.FeatureName, featureName, StringComparison.CurrentCultureIgnoreCase))
                                                 .Any(e => e.Enabled);

    #endregion
}