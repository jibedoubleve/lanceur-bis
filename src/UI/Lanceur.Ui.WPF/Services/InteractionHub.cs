using Lanceur.Core.Services;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public class InteractionHub : IInteractionHub
{
    #region Constructors

    public InteractionHub(
        IUserGlobalNotificationService userGlobalNotificationService,
        IUserInteractionService userInteractionService,
        IUserNotificationService userNotificationService
    )
    {
        GlobalNotifications = userGlobalNotificationService;
        Interactions = userInteractionService;
        Notifications = userNotificationService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public IUserGlobalNotificationService GlobalNotifications { get; }

    /// <inheritdoc />
    public IUserInteractionService Interactions { get; }

    /// <inheritdoc />
    public IUserNotificationService Notifications { get; }

    #endregion
}