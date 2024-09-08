using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters;

public class MahAppsIconNameToSymbolIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string kind)
            return kind switch
            {
                "Settings24"          => "Settings24",
                "AddCircle24"         => "AddCircle24",
                "BookmarkPlusOutline" => "CenterHorizontal24",
                "LocationExit"        => "ArrowExit20",
                "InformationOutline"  => "BookInformation24",
                _                     => "Rocket24"
            };

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}