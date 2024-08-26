using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters
{
    internal class IntegerToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int integer)
            {
                return integer <= 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods
    }
}