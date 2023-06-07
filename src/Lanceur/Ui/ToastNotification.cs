using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.CompilerServices;

namespace Lanceur.Ui
{
    public class ToastNotification : INotification
    {
        #region Methods

        private void Show(string message, [CallerMemberName] string title = null)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }

        public void Error(string message) => Show(message);

        public void Information(string message) => Show(message);

        public void Warning(string message) => Show(message);

        #endregion Methods
    }
}