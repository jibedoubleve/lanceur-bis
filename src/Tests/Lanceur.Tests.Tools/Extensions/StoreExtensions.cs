using System.Web.Bookmarks;
using Everything.Wrapper;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Extensions;
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
            .AddConfigurationSections()
            .AddMockSingleton<IConfigurationFacade>((_, i) => {
                    i.Application.Returns(
                        configuration ??
                        new ApplicationSettings { Caching = new CachingSection(0, 0), Stores = new StoreSection() }
                    );
                    return i;
                }
            );
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