using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Converters;

public class IntegerToVisibilityConverter : IValueConverter
{
    #region Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int integer) return integer >= 0 ? Visibility.Visible : Visibility.Collapsed;

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion Methods
}