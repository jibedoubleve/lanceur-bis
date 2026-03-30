using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public sealed class StoreOrchestrationToStringConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        /* Kept for backward compatibility: shortcuts were previously stored with a full regex
         * (e.g. "^\s{0,}.*value.*"), but now only the plain shortcut string is stored.
         * Strip the legacy regex fragments before returning the value.
         */
        if (value is not string storeOverride) { return Binding.DoNothing; }

        var result = storeOverride.Replace(@"^\s{0,}", "")
                                  .Replace(".*", "");
        return result.Replace("\\", "");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

    #endregion
}