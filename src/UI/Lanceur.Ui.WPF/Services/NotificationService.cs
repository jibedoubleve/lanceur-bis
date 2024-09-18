using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Services;

namespace Lanceur.Ui.WPF.Services;

public class NotificationService : INotificationService
{
    public void Success(string title, string message) => WeakReferenceMessenger.Default.Send(new NotificationMessage((MessageLevel.Success, title, message)));
    public void Warn(string title, string message) => WeakReferenceMessenger.Default.Send(new NotificationMessage((MessageLevel.Warning, title, message)));
}