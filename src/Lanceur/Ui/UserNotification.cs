using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Splat;
using System;

namespace Lanceur.Ui
{
    public class UserNotification : IUserNotification
    {
        #region Fields

        private readonly IAppLogger _log;
        private readonly INotification _notification;

        #endregion Fields

        #region Constructors

        public UserNotification(IAppLoggerFactory logFactory = null, INotification notification = null)
        {
            _log = Locator.Current.GetLogger<UserNotification>(logFactory);
            _notification = notification ?? Locator.Current.GetService<INotification>(); ;
        }

        #endregion Constructors

        #region Methods

        public void Error(string message, Exception ex)
        {
            _log.Error(ex, message);
            _notification.Error(message);
        }

        public void Warning(string message, Exception ex = null)
        {
            _log.Warning(message, ex);
            _notification.Warning(message);
        }

        #endregion Methods
    }
}