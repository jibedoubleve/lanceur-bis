using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

internal class NullToVisibilityConverter : IValueConverter
{
    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    #endregion
}