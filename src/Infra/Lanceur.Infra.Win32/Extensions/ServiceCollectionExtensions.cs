using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Helpers;
using Lanceur.Infra.Win32.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.SharedKernel.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Win32.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddWin32Services(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransientConditional<IProcessLauncher, ProcessLauncherNoOp, ProcessLauncherWin32>()
                         .AddTransient<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<ISteamLibraryService, SteamLibraryService>()
                         .AddThumbnailStrategies()
                         .AddStaThreadRunner();
        return serviceCollection;
    }

    #endregion
}