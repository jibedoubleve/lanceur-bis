using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveMessage(bool value) : ValueChangedMessage<bool>(value);

public class SetQueryMessage(string value) : ValueChangedMessage<string>(value);

public class ChangeCoordinateMessage(Coordinate value) : ValueChangedMessage<Coordinate>(value);

/*
 * MESSAGE BOX MESSAGES
 */
public class QuestionRequestMessage(object content, string title = "Question", string yesTextMessage = "Yes", string noTextMessage = "No") : AsyncRequestMessage<bool>
{
    public string YesText { get; } = yesTextMessage;
    public string NoText { get; } = noTextMessage;

    /// <summary>
    /// Gets the content, which could be either the question text or the UI definition to be displayed in the message box.
    /// </summary>
    public object Content { get; } = content;
    public string Title { get; } = title;
}

/*
 * SNACKBARS
 */
public enum MessageLevel { Success, Warning }

public class NotificationMessage((MessageLevel Level, string Title, string Message) value) : ValueChangedMessage<(MessageLevel Level, string Title, string Message)>(value);

/*
 * NAVIGATION
 */
public class NavigationMessage(Type value) : ValueChangedMessage<Type>(value);