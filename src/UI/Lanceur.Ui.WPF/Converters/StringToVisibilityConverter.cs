using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Ui.WPF.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return Visibility.Collapsed;

        return str.IsNullOrEmpty() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}