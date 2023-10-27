using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Converters
{
    public class InvertVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility
                ? visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible
                : (object)Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods
    }
}