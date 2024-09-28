using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveMessage(bool value) : ValueChangedMessage<bool>(value);

public class SetQueryMessage(string value) : ValueChangedMessage<string>(value);

public class ChangeCoordinateMessage(Coordinate value) : ValueChangedMessage<Coordinate>(value);

/*
 * MESSAGE BOX MESSAGES
 */
public class AskDeleteAlias(string aliasName) : AsyncRequestMessage<bool>
{
    public string AliasName { get; } = aliasName;
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