using Lanceur.Core.Models;
using System.Windows;
using System.Windows.Controls;
using static Lanceur.Views.MainViewModel;

namespace Lanceur.Utils
{
    internal static class UiMixin
    {
        #region Methods
        public static string GetTextFromTextbox(this object source)
        {
            return source is TextBox tb ? tb.Text : string.Empty;
        }
        public static ExecutionRequest GetQueryFromDataContext(this object source)
        {
            var param = source is FrameworkElement e && e?.DataContext is QueryResult result
                ? result?.Query?.ToString() ?? string.Empty
                : string.Empty;
            return new()
            {
                Query = param,
                RunAsAdmin = false,
            };
        }

        #endregion Methods
    }
}