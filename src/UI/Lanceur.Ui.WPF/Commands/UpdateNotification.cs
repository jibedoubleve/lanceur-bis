using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Commands;

public class UpdateNotification
{
    #region Fields

    private readonly ILogger<UpdateNotification> _logger;
    private readonly IUserGlobalNotificationService _notification;

    #endregion

    #region Constructors

    public UpdateNotification(ILogger<UpdateNotification> logger, IUserGlobalNotificationService notification)
    {
        _logger = logger;
        _notification = notification;
    }

    #endregion

    #region Methods

    public void Notify(Version version)
    {
        _logger.LogDebug("New version available. Version {Version}", version);
        _notification.NotifyNewVersionAvailable(version);
    }

    #endregion
}