namespace Lanceur.Ui.Core.Services;

public interface INotificationService
{
    void Success(string title, string message);
    void Warn(string title, string message);
}