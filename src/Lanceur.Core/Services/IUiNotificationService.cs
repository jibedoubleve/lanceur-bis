namespace Lanceur.Core.Services;

public interface IUiNotificationService
{
    #region Methods

    void Error(string message);

    void Information(string message);

    void Warning(string message);

    #endregion
}