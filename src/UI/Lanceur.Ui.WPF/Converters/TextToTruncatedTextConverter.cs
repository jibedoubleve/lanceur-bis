using Humanizer;
using System.Globalization;
using System.Windows.Data;


namespace Lanceur.Ui.WPF.Converters;

public class TextToTruncatedTextConverter : IValueConverter
{
    private const int Length = 70;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return text.Truncate(Length, "(...)");
        }

        return value;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}