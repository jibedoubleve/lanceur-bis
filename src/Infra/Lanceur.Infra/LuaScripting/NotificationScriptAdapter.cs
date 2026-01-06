using Lanceur.Core.Services;

namespace Lanceur.Infra.LuaScripting;

/// <summary>
///     Adapter that gives notification feature that can be used in Lua scripts
/// </summary>
public class NotificationScriptAdapter
{
    #region Fields

    private readonly IUserGlobalNotificationService _notificationService;

    #endregion

    #region Constructors

    public NotificationScriptAdapter(IUserGlobalNotificationService notificationService)
        => _notificationService = notificationService;

    #endregion

    #region Methods

    /// <summary>
    ///     Displays a notification to inform the user of an error and provides additional details about the exception.
    /// </summary>
    /// <param name="message">The error message to be displayed in the notification.</param>
    public void Error(string message) => _notificationService.Error(message);

    /// <summary>
    ///     Displays a notification to inform the user with general information.
    /// </summary>
    /// <param name="message">The informational message to be displayed in the notification.</param>
    public void Information(string message) => _notificationService.Information(message);

    /// <summary>
    ///     Displays an informational message with a navigation button to a related URL.
    ///     The button is labeled "View Details" and triggers navigation to the specified URL when clicked.
    /// </summary>
    /// <param name="message">The message to be displayed in the toast notification.</param>
    /// <param name="url">The URL to navigate to when the user clicks the "View Details" button.</param>
    public  void InformationWithNavigation(string message, string url)
        => _notificationService.InformationWithNavigation(message, url);

    /// <summary>
    ///     Displays a notification to warn the user about a potential issue.
    /// </summary>
    /// <param name="message">The warning message to be displayed in the notification.</param>
    public void Warning(string message) => _notificationService.Warning(message);

    #endregion
}