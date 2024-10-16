namespace Lanceur.Core.Services;

public interface IUserNotificationService
{
    void Success(string message, string title = "Success");
    void Warn(string message, string title = "Warning");
}