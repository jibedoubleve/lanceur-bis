namespace Lanceur.Core.Services;

public interface IUserGlobalNotificationService
{
    #region Methods

    /// <summary>
    ///     Display a notification to ask the user whether a restart of the application should be executed.
    /// </summary>
    void AskRestart();

    /// <summary>
    ///     Displays a notification to inform the user of an error and provides additional details about the exception.
    /// </summary>
    /// <param name="message">The error message to be displayed in the notification.</param>
    /// <param name="ex">The exception object containing detailed information about the error.</param>
    void Error(string message, Exception ex);

    /// <summary>
    ///     Displays a notification to inform the user of an error and provides additional details about the exception.
    /// </summary>
    /// <param name="message">The error message to be displayed in the notification.</param>
    void Error(string message);

    /// <summary>
    ///     Displays a notification to inform the user with general information.
    /// </summary>
    /// <param name="message">The informational message to be displayed in the notification.</param>
    void Information(string message);

    /// <summary>
    ///     Displays an informational message with a navigation button to a related URL.
    ///     The button is labeled "View Details" and triggers navigation to the specified URL when clicked.
    /// </summary>
    /// <param name="message">The message to be displayed in the toast notification.</param>
    /// <param name="url">The URL to navigate to when the user clicks the "View Details" button.</param>
    void InformationWithNavigation(string message, string url);

    /// <summary>
    ///     Displays a notification indicating that a new version of the application is available,
    ///     prompting the user to restart the application to apply the update.
    /// </summary>
    /// <param name="version">The new version that is available.</param>
    void NotifyNewVersionAvailable(Version version);

    /// <summary>
    ///     Starts displaying a visual indicator to inform the user that a background operation is in progress.
    /// </summary>
    void StartBusyIndicator();

    /// <summary>
    ///     Stops displaying the busy indicator, signaling that the background operation has completed.
    /// </summary>
    void StopBusyIndicator();

    /// <summary>
    ///     Displays a notification to warn the user about a potential issue.
    /// </summary>
    /// <param name="message">The warning message to be displayed in the notification.</param>
    void Warning(string message);

    #endregion
}

/// <summary>
///     Contains a set of constants representing the different actions or arguments that can be triggered
///     when interacting with a toast notification that includes buttons.
/// </summary>
public static class ToastNotificationArguments
{
    #region Fields

    /// <summary>
    ///     Indicates that the user requested to navigate to the Github issue.
    /// </summary>
    public const string ClickNavigateIssue = nameof(ClickNavigateIssue);

    /// <summary>
    ///     Indicates that the user requested an application restart
    /// </summary>
    public const string ClickRestart = nameof(ClickRestart);

    /// <summary>
    ///     Indicates that the user requested to view detailed information about the error displayed in the toast notification.
    /// </summary>
    public const string ClickShowError = nameof(ClickShowError);

    /// <summary>
    ///     Indicates that the user requested to open the directory containing the application's log files.
    /// </summary>
    public const string ClickShowLogs = nameof(ClickShowLogs);

    /// <summary>
    ///     Indicates that the user requested to skip the notification of this new version
    /// </summary>
    public const string SkipVersion = nameof(SkipVersion);

    /// <summary>
    ///     Indicates that the user requested a visit of the website
    /// </summary>
    public const string VisitWebsite = nameof(VisitWebsite);

    #endregion
}