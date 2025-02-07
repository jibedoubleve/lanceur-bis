using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;

namespace Lanceur.Tests.Tools.ViewModels;

/// <summary>
///     Manages a collection of visitor functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public class ServiceVisitors
{
    #region Properties

    public IConnectionString? OverridenConnectionString { get; init; }
    public Func<IServiceProvider, IExecutionService, IExecutionService>? VisitExecutionManager { get; init; }
    public Action<ISettingsFacade>? VisitSettings { get; init; }
    public Func<IServiceProvider, IUserInteractionService, IUserInteractionService>? VisitUserInteractionService { get; init; }
    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService>? VisitUserNotificationService { get; init; }

    #endregion
}