using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Core.Configuration;

[Obsolete("Should be moved into tests tools")]
public static class ServiceProviderExtensions
{
    #region Methods

    public static ISection<T>? GetSection<T>(this IServiceProvider serviceProvider)
        where T : class
        => serviceProvider.GetService<ISection<T>>();

    #endregion
}