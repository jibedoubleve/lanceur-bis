using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters
{
    internal class InvertNullToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods
    }
}