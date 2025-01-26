namespace Lanceur.Core.Services;

/// <summary>
/// Represents a hub that centralises all tools for handling user interactions. 
/// This includes managing messages, user input mechanisms, and notifications.
/// </summary>
public interface IUserInteractionHub
{
    #region Properties

    /// <summary>
    /// Provides tools for sending global notifications to users. 
    /// Global notifications are events that notify the user even when they are outside the application context.
    /// </summary>
    IUserGlobalNotificationService GlobalNotifications { get; }

    /// <summary>
    /// Provides tools for handling user input within the application.
    /// </summary>
    IUserInteractionService Interactions { get; }

    /// <summary>
    /// Provides tools for sending notifications to users within the application context.
    /// </summary>
    IUserNotificationService Notifications { get; }

    #endregion
}
