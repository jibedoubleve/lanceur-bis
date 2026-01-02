using Lanceur.Core.Services;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public class UserCommunicationService : IUserCommunicationService
{
    #region Constructors

    public UserCommunicationService(
        IUserGlobalNotificationService userGlobalNotificationService,
        IUserDialogueService userDialogueService,
        IUserNotificationService userNotificationService
    )
    {
        GlobalNotifications = userGlobalNotificationService;
        Dialogues = userDialogueService;
        Notifications = userNotificationService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public IUserGlobalNotificationService GlobalNotifications { get; }

    /// <inheritdoc />
    public IUserDialogueService Dialogues { get; }

    /// <inheritdoc />
    public IUserNotificationService Notifications { get; }

    #endregion
}