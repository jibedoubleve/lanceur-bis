using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.ReservedKeywords;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddWpfServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserGlobalNotificationService, ToastUserNotificationService>()
                         .AddSingleton<IContentDialogService, ContentDialogService>()
                         .AddSingleton<IUserInteractionService, UserUserInteractionService>()
                         .AddSingleton<ISnackbarService, SnackbarService>()
                         .AddSingleton<IUserNotificationService, UserNotificationService>()
                         .AddSingleton<IViewFactory, ViewFactory>()
                         .AddSingleton<IUserInteractionHub, UserInteractionHub>()
                         .AddSingleton(new AssemblySource
                         {
                             ReservedKeywordSource = Assembly.GetAssembly(typeof(QuitAlias)), 
                             MacroSource = Assembly.GetAssembly(typeof(MultiMacro))
                         });

        ConditionalExecution.Set(
            serviceCollection,
            onPrd => onPrd.AddSingleton<IAppRestartService, AppRestartDummyService>(),
            onDbg => onDbg.AddSingleton<IAppRestartService, AppRestartService>()
        );

        return serviceCollection;
    }

    #endregion
}