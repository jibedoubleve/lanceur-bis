#nullable enable
using System.Threading.Tasks;

namespace Lanceur.Core.Services;

public interface IUserInteractionService
{
    Task<bool> AskAsync(string message, string title = "Question", string yes = "Yes", string no = "No");
    
    /// <summary>
    /// Displays a MessageBox with the specified title and content.
    /// Allows customization of the primary button text and an optional secondary button.
    /// </summary>
    /// <param name="title">The title to display in the MessageBox header.</param>
    /// <param name="content">The content to display in the MessageBox body. Accepts strings or other objects.</param>
    /// <param name="ok">The text for the primary button. Defaults to "OK".</param>
    /// <param name="cancel">The text for the optional secondary button. Defaults to null (no secondary button displayed).</param>
    /// <returns>A task that completes when the MessageBox is dismissed.</returns>

    Task ShowAsync(string title, object content, string ok = "Close", string? cancel = null);

    /// <summary>
    /// Asynchronously displays a modal dialog that prompts the user with a Yes/No question.
    /// </summary>
    /// <param name="content">The message or content to be displayed within the modal dialog.</param>
    /// <param name="yesText">The label for the "Yes" button; defaults to "yes" if not provided.</param>
    /// <param name="noText">The label for the "No" button; defaults to "no" if not provided.</param>
    /// <param name="title">The title of the modal dialog; defaults to "Question" if not specified.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result will be 
    /// a boolean value: <c>true</c> if the user selects "Yes", or <c>false</c> if the user selects "No".
    /// </returns>
    Task<bool> AskUserYesNoAsync(object content, string yesText = "Yes", string noText = "No", string title = "Question");

    /// <summary>
    /// Displays an interactive message box to the user, prompting for input.
    /// The message box presents a question or message and allows the user to respond.
    /// </summary>
    /// <param name="content">The content to display in the message box (e.g., a question or form).</param>
    /// <param name="yesText">The text for the confirmation button (default: "Apply").</param>
    /// <param name="noText">The text for the cancellation button (default: "Cancel").</param>
    /// <param name="title">The title of the message box (default: "Interaction").</param>
    /// <param name="dataContext">Optional additional data context associated with the interaction.</param>
    /// <returns>A task returning a tuple where:
    /// - <c>IsConfirmed</c>: A boolean indicating whether the user confirmed (true) or canceled (false).
    /// - <c>DataContext</c>: The provided or modified data context after the interaction.</returns>
    Task<(bool IsConfirmed, object DataContext)> InteractAsync(
        object content, 
        string yesText = "Apply", 
        string noText = "Cancel", 
        string title = "Interaction",  
        object? dataContext = null);

}