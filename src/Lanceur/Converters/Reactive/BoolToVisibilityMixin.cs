using System.Windows;

namespace Lanceur.Converters.Reactive
{
    public static class BoolToVisibilityMixin
    {
        #region Methods

        public static Visibility ToVisibility(this bool value) => value ? Visibility.Visible : Visibility.Collapsed;
        public static Visibility ToVisibility(this int value) => value > 0 ? Visibility.Visible : Visibility.Collapsed;

        #endregion Methods
    }
}