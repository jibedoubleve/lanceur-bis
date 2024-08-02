using System;
using System.Globalization;
using System.Windows.Data;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Converters;

public class StringToStartModeConverter : IValueConverter
{
    #region Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is StartMode startMode)
            return startMode switch
            {
                StartMode.Default   => "Default",
                StartMode.Minimised => "Minimized",
                StartMode.Maximised => "Maximized",
                _                   => null
            };
        else
            return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
            return str switch
            {
                "Default"   => StartMode.Default,
                "Minimized" => StartMode.Minimised,
                "Maximized" => StartMode.Maximised,
                _           => null
            };
        else
            return null;
    }

    #endregion Methods
}