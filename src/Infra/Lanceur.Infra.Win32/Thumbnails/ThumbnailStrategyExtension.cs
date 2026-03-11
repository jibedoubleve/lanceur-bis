using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lanceur.Infra.Win32.Thumbnails;

public static class ThumbnailStrategyExtension
{
    #region Methods

    public static IServiceCollection AddThumbnailStrategies(this IServiceCollection services)
    {
        var assembly = typeof(ThumbnailStrategyExtension).Assembly;
        var types = assembly.GetTypes()
                            .Where(t => t is { IsClass: true, IsAbstract: false }
                                        && t.IsAssignableTo(typeof(IThumbnailStrategy)));

        foreach (var type in types)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IThumbnailStrategy), type));
        }

        return services;
    }

    #endregion
}