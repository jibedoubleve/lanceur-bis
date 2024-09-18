namespace Lanceur.Core.Services;

[Obsolete($"Use {nameof(IUiNotificationService)} instead")]
public interface IUserNotification
{
    #region Methods

    void Error(string message, Exception ex);

    void Warning(string message, Exception ex = null);

    #endregion Methods
}