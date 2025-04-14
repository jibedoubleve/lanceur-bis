using Lanceur.Core.Services;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Commands;

public class UpdateNotification
{
    private readonly ILogger<UpdateNotification> _logger;
    private readonly IUserGlobalNotificationService _notification;

    public UpdateNotification(ILogger<UpdateNotification> logger, IUserGlobalNotificationService notification)
    {
        _logger = logger;
        _notification = notification;
    }
    public void Notify(Version version)
    {
        _logger.LogDebug("New version available. Version {Version}", version);
        _notification.NotifyNewVersionAvailable(version);
    }
}