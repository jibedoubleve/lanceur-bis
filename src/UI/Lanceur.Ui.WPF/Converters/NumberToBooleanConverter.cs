using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class NumberToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int i      => i > 0,
            uint u     => u > 0,
            decimal de => de > 0,
            double d   => d > 0,
            float f    => f > 0,
            _          => value
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}