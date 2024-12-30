using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.SQLite.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbActionFactory, DbActionFactory>();
        return services;
    }

    #endregion
}