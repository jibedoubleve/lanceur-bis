using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Messages;

public class MessageService : IMessageService
{
    #region Methods

    public void Send(object message) => WeakReferenceMessenger.Default.Send(message);

    #endregion
}