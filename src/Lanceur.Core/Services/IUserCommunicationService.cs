namespace Lanceur.Core.Services;

/// <summary>
///     Unified access point aggregating user communication services.
/// </summary>
/// <remarks>
///     This interface aggregates <see cref="IUserDialogueService" /> and <see cref="IUserNotificationService" />
///     to simplify dependency injection and provide a coherent facade to components
///     requiring multiple user communication modes.
/// </remarks>
public interface IUserCommunicationService
{
    #region Properties

    /// <summary>
    ///     Provides tools for handling user input within the application.
    /// </summary>
    IUserDialogueService Dialogues { get; }

    /// <summary>
    ///     Provides tools for sending global notifications to users.
    ///     Global notifications are events that notify the user even when they are outside the application context.
    /// </summary>
    IUserGlobalNotificationService GlobalNotifications { get; }

    /// <summary>
    ///     Provides tools for sending notifications to users within the application context.
    /// </summary>
    IUserNotificationService Notifications { get; }

    #endregion
}