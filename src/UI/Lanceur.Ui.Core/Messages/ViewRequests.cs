using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveMessage(bool value) : ValueChangedMessage<bool>(value);

public class SetQueryMessage(string value) : ValueChangedMessage<string>(value);

public class ChangeCoordinateMessage(Coordinate value) : ValueChangedMessage<Coordinate>(value);

/*
 * MESSAGE BOX MESSAGES
 */
public class Ask(string message, string title = "Question", string yesTextMessage = "Yes", string noTextMessage = "No") : AsyncRequestMessage<bool>
{
    public string YesText { get; } = yesTextMessage;
    public string NoText { get; } = noTextMessage;
    public string Question { get; } = message;
    public string Title { get; } = title;
    public static implicit operator Ask(string source) => new(source);
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