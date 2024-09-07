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

        var direction = keyEventArgs.Key switch
        {
            Key.Up       => Direction.Up,
            Key.Down     => Direction.Down,
            Key.PageUp   => Direction.PageUp,
            Key.PageDown => Direction.PageDown,
            _            => Direction.None
        };

        /*
         * If the direction is "none", the event should be handled elsewhere.
         * Otherwise, it indicates navigation within the results, and executing
         * the command is the only behavior required.
         */
        if (direction !=  Direction.None) keyEventArgs.Handled = true;
        return direction;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}