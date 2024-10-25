using System.Globalization;
using System.Windows.Data;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Converters;

public class InvertPlotTypeToBooleanConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not PlotType plot || parameter is not PlotType expectedType
        ? value
        : plot != expectedType;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}