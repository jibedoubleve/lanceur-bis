using System.Web.Bookmarks;
using Everything.Wrapper;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lanceur.Tests.Tools.Extensions;

public static class StoreExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        #region Methods

        public IServiceCollection AddStoreServicesConfiguration(ApplicationSettings? configuration = null)
        {
            serviceCollection
                .AddConfigurationSections()
                .AddMockSingleton<IConfigurationFacade>((_, i) =>
                    {
                        i.Application.Returns(
                            configuration ??
                            new ApplicationSettings { Caching = new(0, 0), Stores = new() }
                        );
                        return i;
                    }
                );
            return serviceCollection;
        }

        public IServiceCollection  AddStoreServicesMockContext()
        {
            serviceCollection
                .AddMockSingleton<IAliasRepository>()
                .AddMockSingleton<IFeatureFlagService>()
                .AddMockSingleton<IBookmarkRepositoryFactory>()
                .AddMockSingleton<ICalculatorService>()
                .AddMockSingleton<AssemblySource>()
                .AddMockSingleton<IEverythingApi>();
            return serviceCollection;
        }

        #endregion
    }
}