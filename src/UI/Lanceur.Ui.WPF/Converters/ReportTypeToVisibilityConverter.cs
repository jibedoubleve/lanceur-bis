using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Converters;

public class ReportTypeToVisibilityConverter : IValueConverter
{
    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ReportType report) return Binding.DoNothing;
        if (parameter is not string paramString) return Binding.DoNothing;

        if (Enum.TryParse(typeof(ReportType), paramString, out var paramValue) && paramValue is ReportType enumValue)
            return report == enumValue
                ? Visibility.Visible
                : Visibility.Collapsed;

        return Binding.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}