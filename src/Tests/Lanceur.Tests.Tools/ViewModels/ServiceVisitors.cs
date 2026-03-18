using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Repositories;
using Lanceur.Ui.Core.Utils;

namespace Lanceur.Tests.Tools.ViewModels;

/// <summary>
///     Manages a collection of visitor functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public sealed class ServiceVisitors
{
    #region Properties

    public IConnectionString? OverridenConnectionString { get; init; }

    public Func<IServiceProvider, IUserGlobalNotificationService, IUserGlobalNotificationService>?
        VisitGlobalUserInteractionService { get; init; }

    public Func<IServiceProvider, IProcessLauncher, IProcessLauncher>? VisitProcessLauncher { get; init; }
    
    public Action<MemoryApplicationSettingsProvider>? VisitApplicationSettingsProvider { get; init; }

    public Func<IServiceProvider, IUserDialogueService, IUserDialogueService>? VisitUserInteractionService
    {
        get;
        init;
    }

    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService>? VisitUserNotificationService
    {
        get;
        init;
    }

    public Func<IServiceProvider, IViewFactory, IViewFactory>? VisitViewFactory { get; init; }

    #endregion
}