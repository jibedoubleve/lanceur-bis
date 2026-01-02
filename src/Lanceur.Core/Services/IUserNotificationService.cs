namespace Lanceur.Core.Services;

/// <summary>
///     Provides unidirectional notification mechanisms to the user without interrupting the execution flow.
/// </summary>
/// <remarks>
///     This service handles passive notifications such as toasts, non-blocking alerts,
///     and visual indicators (cursor, badges) requiring no user interaction.
///     Use <see cref="IUserDialogueService" /> for interactions requiring explicit response.
/// </remarks>
public interface IUserNotificationService
{
    #region Methods

    /// <summary>
    ///     Notifies the user that the background process has finished loading.
    ///     For example, this can restore the mouse cursor to its default state.
    /// </summary>
    void DisableLoadingState();

    /// <summary>
    ///     Notifies the user that a process is loading in the background.
    ///     This is intended to provide feedback to the UI, such as displaying a loading indicator (e.g., a mouse sandglass).
    /// </summary>
    void EnableLoadingState();

    /// <summary>
    ///     Displays a success notification to the user.
    /// </summary>
    /// <param name="message">The message to display in the notification.</param>
    /// <param name="title">The title of the notification. Defaults to "Success".</param>
    void Success(string message, string title = "Success");

    /// <summary>
    ///     Creates a scope that notifies the user that a background process is loading.
    ///     When this loading scope is disposed, the user is automatically notified that
    ///     the background process has completed.
    ///     Usage:
    ///     - Instantiate this scope at the start of the background operation.
    ///     - Dispose of it once the operation is finished to signal completion.
    ///     This helps provide feedback to the user about the progress of asynchronous tasks.
    /// </summary>
    IDisposable TrackLoadingState();

    /// <summary>
    ///     Displays a warning notification to the user.
    /// </summary>
    /// <param name="message">The message to display in the notification.</param>
    /// <param name="title">The title of the notification. Defaults to "Warning".</param>
    void Warning(string message, string title = "Warning");

    #endregion
}