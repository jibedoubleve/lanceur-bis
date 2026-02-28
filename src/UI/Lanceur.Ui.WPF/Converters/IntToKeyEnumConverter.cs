using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Lanceur.Ui.WPF.Converters;

public class IntToKeyEnumConverter : IValueConverter
{
    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int integer) { return Binding.DoNothing; }

        return (Key)integer;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Key key) { return Binding.DoNothing; }

        return (int)key;
    }

    #endregion
}