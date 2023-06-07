namespace Lanceur.Ui
{
    public interface INotification
    {
        void Error(string message);
        void Information(string message);
        void Warning(string message);
    }
}