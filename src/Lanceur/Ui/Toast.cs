using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.CompilerServices;

namespace Lanceur.Ui
{
    public static class Toast
    {
        #region Methods

        public static void Error(string message) => Show(message);

        public static void Information(string message) => Show(message);

        public static void Show(string message, [CallerMemberName] string title = null)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }

        public static void Warning(string message) => Show(message);

        #endregion Methods
    }
}