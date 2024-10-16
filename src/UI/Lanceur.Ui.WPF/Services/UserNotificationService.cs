using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;

namespace Lanceur.Ui.WPF.Services;

public class UserNotificationService : IUserNotificationService
{
    #region Methods

    private void Send(MessageLevel level, string title, string message) => WeakReferenceMessenger.Default.Send(new NotificationMessage((level, title, message)));

    public void Success(string message, string title) => Send(MessageLevel.Success, title, message);
    public void Warn(string message, string title) => Send(MessageLevel.Warning, title, message);

    #endregion
}