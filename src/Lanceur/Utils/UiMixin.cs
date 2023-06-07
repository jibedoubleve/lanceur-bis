using Lanceur.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lanceur.Utils
{
    internal static class UiMixin
    {
        #region Methods
        public static T GetParentDataSource<T>(this object source)
        {
            var parent = VisualTreeHelper.GetParent(source as DependencyObject);
            return (T)(parent as FrameworkElement)?.DataContext;
        }
        public static string GetTextFromTextbox(this object source)
        {
            return source is TextBox tb ? tb.Text : string.Empty;
        }

        #endregion Methods
    }
}