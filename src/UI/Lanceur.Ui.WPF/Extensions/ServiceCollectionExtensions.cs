using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Services;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.WPF.Commands;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.ReservedKeywords;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static  IServiceCollection AddCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<UpdateNotification>();
        return serviceCollection;
    }

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
                         .AddSingleton<IInteractionHubService, InteractionHubService>()
                         .AddSingleton<IReleaseService, ReleaseService>()
                         .AddSingletonConditional<IAppRestartService, AppRestartDummyService, AppRestartService>()
                         .AddSingleton(
                             new AssemblySource
                             {
                                 ReservedKeywordSource = Assembly.GetAssembly(typeof(QuitAlias)),
                                 MacroSource = Assembly.GetAssembly(typeof(MultiMacro))
                             }
                         );

        return serviceCollection;
    }

    #endregion
}