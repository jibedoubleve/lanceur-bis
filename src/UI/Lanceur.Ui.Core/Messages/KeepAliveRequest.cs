using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveRequest(bool value) : ValueChangedMessage<bool>(value);