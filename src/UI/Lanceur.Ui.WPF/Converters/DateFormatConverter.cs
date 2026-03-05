using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class DateFormatConverter : IMultiValueConverter
{
    #region Methods

    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values is [DateTime dateTime, string format]
            ? dateTime.ToString(format)
            : Binding.DoNothing;

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    #endregion
}