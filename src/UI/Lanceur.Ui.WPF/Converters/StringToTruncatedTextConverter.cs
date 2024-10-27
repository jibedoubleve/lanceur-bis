using Humanizer;
using System.Globalization;
using System.Windows.Data;


namespace Lanceur.Ui.WPF.Converters;

public class TextToTruncatedTextConverter : IValueConverter
{
    #region Fields

    private const int Length = 70;

    #endregion

    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text) return text.Truncate(Length, "(...)");

        return Binding.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}