using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Lanceur.Ui.Core;

namespace Lanceur.Ui.WPF.Converters;

public class KeyToDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not KeyEventArgs keyEventArgs) return value;

        return keyEventArgs.Key switch
        {
            Key.Up       => Direction.Up,
            Key.Down     => Direction.Down,
            Key.PageUp   => Direction.PageUp,
            Key.PageDown => Direction.PageDown,
            _            => Direction.None,
        };

    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}