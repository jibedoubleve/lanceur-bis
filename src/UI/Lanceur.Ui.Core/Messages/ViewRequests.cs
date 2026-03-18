using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.Constants;

namespace Lanceur.Ui.Core.Messages;

public sealed class KeepAliveMessage(bool value) : ValueChangedMessage<bool>(value);

public sealed class SetQueryMessage(string value) : ValueChangedMessage<string>(value);

public sealed class SaveAliasMessage(AliasQueryResult alias) : ValueChangedMessage<AliasQueryResult>(alias);

public sealed class ChangeCoordinateMessage(Coordinate value) : ValueChangedMessage<Coordinate>(value);

/*
 * MESSAGE BOX MESSAGES
 */
public sealed class QuestionRequestMessage(
    object content,
    string title = "Question",
    string yesTextMessage = ButtonLabels.Yes,
    string noTextMessage = ButtonLabels.No
) : AsyncRequestMessage<bool>
{
    #region Properties

    /// <summary>
    ///     Gets the content, which could be either the question text or the UI definition to be displayed in the message box.
    /// </summary>
    public object Content { get; } = content;

    public string NoText { get; } = noTextMessage;
    public string Title { get; } = title;
    public string YesText { get; } = yesTextMessage;

    #endregion
}

/*
 * SNACKBARS
 */
public enum MessageLevel { Success, Warning }

public sealed class NotificationMessage((MessageLevel Level, string Title, string Message) value)
    : ValueChangedMessage<(MessageLevel Level, string Title, string Message)>(value);

/*
 * NAVIGATION
 */
public sealed class NavigationMessage((Type ViewType, object? DataContext) value)
    : ValueChangedMessage<(Type ViewType, object? DataContext)>(value);