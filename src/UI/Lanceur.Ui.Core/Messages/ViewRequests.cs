using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveMessage(bool value) : ValueChangedMessage<bool>(value);
public class SetQueryMessage(string value) : ValueChangedMessage<string>(value);
public class ChangeCoordinateMessage(Coordinate coordinate) : ValueChangedMessage<Coordinate>(coordinate);