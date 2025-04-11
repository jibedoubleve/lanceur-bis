using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using INavigationService = Lanceur.Core.Services.INavigationService;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;
using NavigationService = Lanceur.Ui.WPF.Services.NavigationService;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddWpfServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserGlobalNotificationService, ToastUserNotificationService>()
                         .AddSingleton<IComputerInfoService, ComputerInfoService>()
                         .AddSingleton<IContentDialogService, ContentDialogService>()
                         .AddSingleton<IUserInteractionService, UserInteractionService>()
                         .AddSingleton<ISnackbarService, SnackbarService>()
                         .AddSingleton<IUserNotificationService, UserNotificationService>()
                         .AddSingleton<IViewFactory, ViewFactory>()
                         .AddSingleton<IHotKeyService, HotKeyService>()
                         .AddSingleton<IInteractionHub, InteractionHub>()
                         .AddSingleton<INavigationService, NavigationService>()
                         .AddSingleton<IApplicationService, ApplicationService>();

        ConditionalExecution.Execute(
            serviceCollection,
            onPrd => onPrd.AddSingleton<IAppRestartService, AppRestartDummyService>(),
            onDbg => onDbg.AddSingleton<IAppRestartService, AppRestartService>()
        );

        return serviceCollection;
    }

    #endregion
}