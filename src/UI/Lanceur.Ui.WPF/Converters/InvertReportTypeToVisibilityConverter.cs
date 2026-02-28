using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class InvertReportTypeToVisibilityConverter : IValueConverter
{
    #region Fields

    private  static readonly ReportTypeToVisibilityConverter Instance = new();

    #endregion

    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Instance.Convert(
                value,
                targetType,
                parameter,
                culture
            ) is Visibility visibility)
        {
            return visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        return Binding.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    #endregion
}