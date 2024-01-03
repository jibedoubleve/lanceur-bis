using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using System;

namespace Lanceur.Ui
{
    public class UserNotification : IUserNotification
    {
        #region Fields

        private readonly ILogger<UserNotification> _logger;
        private readonly INotification _notification;

        #endregion Fields

        #region Constructors

        public UserNotification(ILoggerFactory logFactory = null, INotification notification = null)
        {
            _logger = logFactory.GetLogger<UserNotification>();
            _notification = notification ?? Locator.Current.GetService<INotification>(); ;
        }

        #endregion Constructors

        #region Methods

        //TODO: refactor logging... It's not optimised
        public void Error(string message, Exception ex)
        {
            _logger.LogError(ex, message);
            _notification.Error(message);
        }

        //TODO: refactor logging... It's not optimised
        public void Warning(string message, Exception ex = null)
        {
            _logger.LogWarning(message, ex);
            _notification.Warning(message);
        }

        #endregion Methods
    }
}