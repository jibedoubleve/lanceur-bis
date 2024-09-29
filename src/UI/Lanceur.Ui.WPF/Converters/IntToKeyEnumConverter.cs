using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Lanceur.Ui.WPF.Converters;

public class IntToKeyEnumConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int integer) return value;

        return (Key)integer;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Key key) return value;

        return (int)key;
    }

    #endregion
}