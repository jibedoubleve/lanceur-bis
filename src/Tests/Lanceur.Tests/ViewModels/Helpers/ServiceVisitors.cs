using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;

namespace Lanceur.Tests.ViewModels.Helpers;

/// <summary>
///     Manages a collection of visitor functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public class ServiceVisitors
{
    #region Properties

    public Func<IServiceProvider, IExecutionService, IExecutionService> VisitExecutionManager { get; init; }
    public Action<ISettingsFacade> VisitSettings { get; init; }
    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService> VisitUserNotificationService { get; init; }
    public Func<IServiceProvider, IUserInteractionService, IUserInteractionService> VisitUserInteractionService { get; init; }

    #endregion
}