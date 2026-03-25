using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
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

    private ApplicationSettings? _current;

    private readonly JsonSerializerSettings _jsonSettings
        = new() { ObjectCreationHandling = ObjectCreationHandling.Replace };

    private readonly ILogger<SQLiteApplicationSettingsProvider> _logger;

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
            if (_current is null) { Load(); }

            return _current!;
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

        var value = json.IsNullOrEmpty()
            ? new ApplicationSettings()
            : JsonConvert.DeserializeObject<ApplicationSettings>(json, _jsonSettings)
              ?? new ApplicationSettings();
        value.AddNewFeatureFlags();
        _current = value;
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