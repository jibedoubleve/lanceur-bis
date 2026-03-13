using System.Web.Bookmarks;
using Everything.Wrapper;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lanceur.Tests.Tools.Extensions;

public static class StoreExtensions
{
    #region Methods

    public static IServiceCollection AddStoreServicesConfiguration(
        this IServiceCollection serviceCollection,
        ApplicationSettings? configuration = null)
    {
        serviceCollection
            .AddMockConfigurationSections(s => {
                s.Current.Caching = configuration is null 
                    ? new CachingSection(0, 0) 
                    : configuration.Caching;
                
                s.Current.Stores = configuration is null 
                    ? new StoreSection() 
                    : configuration.Stores;
            });
        return serviceCollection;
    }

    public static IServiceCollection AddStoreServicesMockContext(
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, ISection<StoreSection>, ISection<StoreSection>> configurator)
    {
        serviceCollection
            .AddMockSingleton<IAliasRepository>()
            .AddMockSingleton<IFeatureFlagService>()
            .AddMockSingleton<IBookmarkRepositoryFactory>()
            .AddMockSingleton<ICalculatorService>()
            .AddMockSingleton<AssemblySource>()
            .AddMockSingleton<ISteamLibraryService>()
            .AddMockSingleton<IAliasManagementService>()
            .AddMockSingleton<IEverythingApi>()
            .AddMockSingleton(configurator);
        return serviceCollection;
    }

    public static IServiceCollection AddStoreServicesMockContext(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddStoreServicesMockContext((_, i) => {
            i.Value.Returns(new StoreSection());
            return i;
        });
        return serviceCollection;
    }

    #endregion
}