using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Repositories;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Tests.Tools.ViewModels;

/// <summary>
///     Manages a collection of hooks functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public sealed class ServiceHooks
{
    #region Properties

    public Action<MemoryApplicationSettingsProvider>? InterceptApplicationSettingsProvider { get; init; }

    public Func<IServiceProvider, IUserGlobalNotificationService, IUserGlobalNotificationService>?
        InterceptGlobalUserInteractionService { get; init; }

    public Func<IServiceProvider, IProcessLauncher, IProcessLauncher>? InterceptProcessLauncher { get; init; }

    public Func<IServiceProvider, IUserDialogueService, IUserDialogueService>? InterceptUserInteractionService
    {
        get;
        init;
    }

    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService>? InterceptUserNotificationService
    {
        get;
        init;
    }

    public Func<IServiceProvider, IViewFactory, IViewFactory>? InterceptViewFactory { get; init; }

    public IConnectionString? OverridenConnectionString { get; init; }

    public Action<IServiceCollection>? OverridenServices { get; init; }

    public IWatchdogBuilder? OverridenWatchdogBuilder { get; init; }

    #endregion
}