namespace Lanceur.Core.Services;

[Obsolete($"Use {nameof(IUiNotification)} instead")]
public interface IUiNotification
{
    #region Methods

    void Error(string message, Exception ex);

    void Warning(string message, Exception ex = null);

    #endregion Methods
}