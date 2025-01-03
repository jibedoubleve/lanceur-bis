using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Converters;

public class IsDoubloonReportToVisibilityConverter : IValueConverter
{
    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ReportType report)
            return report == ReportType.DoubloonAliases
                ? Visibility.Visible
                : Visibility.Collapsed;

        return Binding.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}