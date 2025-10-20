using Lanceur.Core.Configuration.Sections;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Core.Configuration;

public static class ServiceProviderExtensions
{
    #region Methods

    public static ISection<T> GetSection<T>(this IServiceProvider serviceProvider)
        where T : class => serviceProvider.GetService<ISection<T>>();

    public static IWriteableSection<T> GetWriteableSection<T>(this IServiceProvider serviceProvider)
        where T : class => serviceProvider.GetService<IWriteableSection<T>>();

    #endregion
}