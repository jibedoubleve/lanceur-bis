using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class StoreTypeToStoreNameConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Type t) return t.Name.Replace("Store", "");

        return Binding.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}