using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite.Repositories;

/// <inheritdoc cref="ISettingsProvider{ApplicationSettings}" />
public sealed class SQLiteApplicationSettingsProvider : SQLiteRepositoryBase, ISettingsProvider<ApplicationSettings>
{
    #region Fields

    private readonly ILogger<SQLiteApplicationSettingsProvider> _logger;

    private static readonly JsonSerializerSettings SerializerSettings =
        new() { ObjectCreationHandling = ObjectCreationHandling.Replace };

    private bool _settingsLoaded;

    private readonly ApplicationSettings _value = new();

    #endregion

    #region Constructors

    public SQLiteApplicationSettingsProvider(
        IDbConnectionManager manager,
        ILogger<SQLiteApplicationSettingsProvider> logger
    ) : base(manager)
        => _logger = logger;

    #endregion

    #region Properties

    /// <inheritdoc />
    object ISettingsProvider.Value => Value;

    /// <inheritdoc />
    public ApplicationSettings Value
    {
        get
        {
            if (_settingsLoaded) { return _value; }

            Load();
            _settingsLoaded = true;

            return _value!;
        }
    }

    #endregion

    #region Methods

    /// <inheritdoc />
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
                .FirstOrDefault() ?? string.Empty
        );

        if (json.IsNullOrEmpty())
        {
            _logger.LogWarning("No settings found in the database. Skipping settings load.");
            return;
        }

        _value.Stores.StoreShortcuts = [];
        JsonConvert.PopulateObject(json, _value, SerializerSettings);
        _value.AddNewFeatureFlags();
    }

    /// <inheritdoc />
    public void Save()
    {
        const string sql = """
                           insert into settings(s_key, s_value) values ('json', @json)
                           on conflict (s_key) do update 
                           set 
                           	s_value = @json
                           where s_key = 'json'
                           """;
        var json = JsonConvert.SerializeObject(Value);

        Db.WithConnection(conn => conn.Execute(sql, new { json }));
        _logger.LogTrace("Saved settings in database.");
    }

    #endregion
}