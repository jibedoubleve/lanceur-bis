using System.Data;
using System.Data.SQLite;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.ConnectionStrings;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Scripts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                .AddTransient<IChartDefinitionRepository, SQLiteChartDefinitionRepository>()
                .AddTransient<IDataStoreVersionService, SQLiteVersionService>();
        return services;
    }

    #endregion
}