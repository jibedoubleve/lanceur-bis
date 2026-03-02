using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Ui.WPF.Converters;

internal class DebugToVisibilityConverter : IValueConverter
{
    #region Fields

    private readonly Conditional<Visibility> _conditional = new(Visibility.Visible, Visibility.Collapsed);

    #endregion

    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility ? _conditional : Binding.DoNothing;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    #endregion
}