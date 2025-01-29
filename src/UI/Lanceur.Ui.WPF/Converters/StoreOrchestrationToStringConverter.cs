using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class StoreOrchestrationToStringConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string storeOverride)
            return storeOverride.Replace(@"^\s{0,}", "")
                                .Replace(".*", "");

        return Binding.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string storeOverride) return $@"^\s{{0,}}{storeOverride}.*";

        return Binding.DoNothing;
    }

    #endregion
}