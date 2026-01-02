using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite.Repositories;

/// <inheritdoc cref="IApplicationSettingsProvider" />
public class SQLiteApplicationSettingsProvider : SQLiteRepositoryBase, IApplicationSettingsProvider
{
    #region Fields

    private ApplicationSettings _current;

    private readonly JsonSerializerSettings _jsonSettings
        = new() { ObjectCreationHandling = ObjectCreationHandling.Replace };

    private readonly ILogger<SQLiteApplicationSettingsProvider> _logger;

    #endregion

    #region Constructors

    public SQLiteApplicationSettingsProvider(
        IDbConnectionManager manager,
        ILogger<SQLiteApplicationSettingsProvider> logger
    ) : base(manager) => _logger = logger;

    #endregion

    #region Properties

    public ApplicationSettings Current
    {
        get
        {
            if (_current is null) Load();
            return _current;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     HACK: If there's new feature flags added in a new version of the application,
    ///     add it here.
    /// </summary>
    private ApplicationSettings AddNewFeatureFlags(ApplicationSettings config)
    {
        if (config is null) return null;

        var defaultFf = new ApplicationSettings().FeatureFlags; //Default featureFlags
        var currentFf = new List<FeatureFlag>(config.FeatureFlags);

        var newFf = defaultFf.Where(f => config.FeatureFlags.All(c => c.FeatureName != f.FeatureName));

        currentFf.AddRange(newFf);
        config.FeatureFlags = currentFf;
        return config;
    }

    /// <inheritdoc/>
    public void Edit(Action<ApplicationSettings> edit)
    {
        var stg = Current;
        edit(stg);
        Save();
    }

    /// <inheritdoc/>
    public void Load()
    {
        const string sql = """
                           select
                               s_value as Value
                           from settings
                           where s_key = 'json';
                           """;
        var json = Db.WithConnection(conn =>
            conn.Query<string>(sql)
                .FirstOrDefault()
        );

        _current =  AddNewFeatureFlags(
            json.IsNullOrEmpty()
                ? new()
                : JsonConvert.DeserializeObject<ApplicationSettings>(json, _jsonSettings)
        );
    }

    /// <inheritdoc/>
    public void Save()
    {
        Db.WithinTransaction(tx =>
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
        _logger.LogTrace("Saved settings in database.");
    }

    #endregion
}