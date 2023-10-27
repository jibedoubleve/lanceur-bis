using System.Windows;

namespace Lanceur.Converters.Reactive
{
    internal static class VisbilityConvertersMixin
    {
        #region Methods

        public static Visibility ToVisibility(this bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility ToVisibility(this int value) => value > 0 ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility ToVisibilityInverted(this bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        #endregion Methods
    }
}