using Lanceur.Infra.Win32.Thumbnails.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Infra.Win32.Thumbnails;

public static class ThumbnailStrategyExtension
{
    #region Methods

    public static IServiceCollection AddThumbnailStrategies(this IServiceCollection services)
    {
        Type[] types =
        [
            typeof(FavIconAppThumbnailStrategy),
            typeof(Win32AppThumbnailStrategy),
            typeof(PackagedAppThumbnailStrategy)
        ];

        foreach (var type in types)
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IThumbnailStrategy), type));

        return services;
    }

    #endregion
}