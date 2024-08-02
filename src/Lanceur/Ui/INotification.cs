namespace Lanceur.Ui;

public interface INotification
{
    #region Methods

    void Error(string message);

    void Information(string message);

    void Warning(string message);

    #endregion Methods
}