using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class DivideByConverter : IValueConverter
{
    #region Properties

    /// <summary>
    /// Specifies the divider used in the <see cref="Convert"/> method for value transformation.
    /// </summary>
    /// <remarks>
    /// If this property is set to <c>null</c>, the converter will fall back to using the default behavior
    /// and rely on any parameter value provided directly in the XAML.
    /// </remarks>
    public int? Divider { get; set; }

    #endregion

    #region Methods

    private static double Divide(int integer, int divider)
    {
        var number = (float)integer / divider;
        return Math.Ceiling(number * 2) / 2;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!int.TryParse($"{value}", out var integer)) return Binding.DoNothing;

        if (Divider is not null) return Divide(integer, Divider.Value);
        if (!int.TryParse($"{parameter}", out var divider)) return Binding.DoNothing;
        if (divider == 0) return integer;

        return Divide(integer, divider);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}