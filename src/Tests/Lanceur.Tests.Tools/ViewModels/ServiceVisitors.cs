using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Ui.Core.Utils;

namespace Lanceur.Tests.Tools.ViewModels;

/// <summary>
///     Manages a collection of visitor functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public class ServiceVisitors
{
    #region Properties

    public IConnectionString? OverridenConnectionString { get; init; }
    public Func<IServiceProvider, IProcessLauncher, IProcessLauncher>? VisitProcessLauncher { get; init; }
    public Func<IServiceProvider, IUserGlobalNotificationService, IUserGlobalNotificationService>? VisitGlobalUserInteractionService { get; init; }
    public Action<IConfigurationFacade>? VisitSettings { get; init; }
    public Func<IServiceProvider, IUserInteractionService, IUserInteractionService>? VisitUserInteractionService { get; init; }
    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService>? VisitUserNotificationService { get; init; }
    public Func<IServiceProvider, IViewFactory, IViewFactory>? VisitViewFactory { get; init; }

    #endregion
}