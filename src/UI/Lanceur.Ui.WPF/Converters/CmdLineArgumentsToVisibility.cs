using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Converters;

public class CmdLineArgumentsToVisibility : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return Binding.DoNothing;

        var cmdline = Cmdline.Parse(str);
        return cmdline.Parameters.Any() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}