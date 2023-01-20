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

        #endregion Fields

        #region Constructors

        public UserNotification(IAppLoggerFactory logFactory = null)
        {
            _log = Locator.Current.GetLogger<UserNotification>(logFactory);
        }

        #endregion Constructors

        #region Methods

        public void Error(string message, Exception ex)
        {
            _log.Error(message, ex);
            Toast.Error(message);
        }

        public void Warning(string message, Exception ex = null)
        {
            _log.Warning(message, ex);
            Toast.Warning(message);
        }

        #endregion Methods
    }
}