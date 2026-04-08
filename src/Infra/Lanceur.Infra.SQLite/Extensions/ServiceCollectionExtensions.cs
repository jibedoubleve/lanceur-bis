using System.Data;
using System.Data.SQLite;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Scripts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lanceur.Infra.SQLite.ConnectionStrings;

namespace Lanceur.Infra.SQLite.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbActionFactory, DbActionFactory>()
                .AddTransient<IDbConnection, SQLiteConnection>(sp
                    => new SQLiteConnection(sp.GetService<IConnectionString>()!.ToString())
                )
                .AddTransient<IDatabaseUpdater>(sp => new SQLiteUpdater(
                        sp.GetService<IDataStoreVersionService>()!,
                        sp.GetService<ILoggerFactory>()!,
                        sp.GetService<IDbConnection>()!,
                        ScriptRepository.Asm,
                        ScriptRepository.DbScriptEmbeddedResourcePattern
                    )
                )
                .AddTransient<IAliasRepository, SQLiteAliasRepository>()
                .AddTransient<IConnectionString, ConnectionString>()
                .AddTransient<IDbConnectionManager, DbMultiConnectionManager>()
                .AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>()
                .AddTransient<IFeatureFlagRepository, SQLiteFeatureFlagRepository>()
                .AddTransient<IDataStoreVersionService, SQLiteVersionService>()
                .AddSingleton<ISettingsProvider<ApplicationSettings>, SQLiteApplicationSettingsProvider>();
        return services;
    }

    #endregion
}