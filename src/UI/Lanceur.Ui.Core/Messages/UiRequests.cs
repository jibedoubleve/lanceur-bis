using CommunityToolkit.Mvvm.Messaging.Messages;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Messages;

public class KeepAliveRequest(bool value) : ValueChangedMessage<bool>(value);
public class ChangeCoordinateRequest(Coordinate coordinate) : ValueChangedMessage<Coordinate>(coordinate);