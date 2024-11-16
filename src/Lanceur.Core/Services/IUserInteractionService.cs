#nullable enable
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
    /// <param name="dataContext">Optional parameter for the data context that can be bound to the modal dialog.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result will be 
    /// a boolean value: <c>true</c> if the user selects "Yes", or <c>false</c> if the user selects "No".
    /// </returns>
    Task<bool> AskUserYesNoAsync(object content, string yesText = "yes", string noText = "no", string title = "Question", object? dataContext = null);
}