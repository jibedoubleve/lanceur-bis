using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Tests.Tooling;
using Lanceur.Ui.Core.Utils.ConnectionStrings;

namespace Lanceur.Tests.ViewModels.Helpers;

/// <summary>
///     Manages a collection of visitor functions that allow users to configure
///     custom behaviour for the <c>serviceProvider</c> with specific types.
/// </summary>
public class ServiceVisitors
{
    #region Properties

    public IConnectionString OverridenConnectionString { get; init; }
    public Func<IServiceProvider, IExecutionService, IExecutionService> VisitExecutionManager { get; init; }
    public Action<ISettingsFacade> VisitSettings { get; init; }
    public Func<IServiceProvider, IUserNotificationService, IUserNotificationService> VisitUserNotificationService { get; init; }
    public Func<IServiceProvider, IUserInteractionService, IUserInteractionService> VisitUserInteractionService { get; init; }

    #endregion
}