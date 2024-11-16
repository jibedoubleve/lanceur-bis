namespace Lanceur.Core.Services;

public interface IUserNotificationService
{
    /// <summary>
    /// Notifies the user that a process is loading in the background. 
    /// This is intended to provide feedback to the UI, such as displaying a loading indicator (e.g., a mouse sandglass).
    /// </summary>
    void BeginLoading();

    /// <summary>
    /// Notifies the user that the background process has finished loading. 
    /// For example, this can restore the mouse cursor to its default state.
    /// </summary>
    void EndLoading();
    
    /// <summary>
    /// Displays a success notification to the user.
    /// </summary>
    /// <param name="message">The message to display in the notification.</param>
    /// <param name="title">The title of the notification. Defaults to "Success".</param>
    void Success(string message, string title = "Success");

    /// <summary>
    /// Displays a warning notification to the user.
    /// </summary>
    /// <param name="message">The message to display in the notification.</param>
    /// <param name="title">The title of the notification. Defaults to "Warning".</param>
    void Warn(string message, string title = "Warning");
}