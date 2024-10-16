namespace Lanceur.Core.Services;

public interface IUserGlobalNotificationService
{
    void Error(string message);
    void Information(string message);
    void Warning(string message);
}