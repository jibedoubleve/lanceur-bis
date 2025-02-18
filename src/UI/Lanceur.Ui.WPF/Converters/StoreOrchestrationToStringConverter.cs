using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class StoreOrchestrationToStringConverter : IValueConverter
{
    #region Fields

    /// <remarks>
    /// The backslash should be the first item in the list otherwise it'll be escaped multiple times...
    /// </remarks>
    private readonly string[] _toEscape = ["\\", ".", "*", "+", "?", "{", "}", "[", "]", "(", ")", "`", "^", "$"];

    #endregion

    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string storeOverride) return Binding.DoNothing;

        return storeOverride.Replace(@"^\s{0,}", "")
                            .Replace(".*", "");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string storeOverride) return Binding.DoNothing;
        
        foreach (var character in _toEscape) storeOverride = storeOverride.Replace(character, $"\\{character}");

        return $@"^\s{{0,}}{storeOverride}.*";
    }

    #endregion
}