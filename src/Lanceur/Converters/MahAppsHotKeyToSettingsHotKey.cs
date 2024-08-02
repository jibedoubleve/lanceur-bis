using Lanceur.Core.Models.Settings;
using MahApps.Metro.Controls;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Lanceur.Converters;

public class MahAppsHotKeyToSettingsHotKey : IValueConverter
{
    #region Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HotKeySection hk)
        {
            var r = new HotKey((Key)hk.Key, (ModifierKeys)hk.ModifierKey);
            return r;
        }
        else { return value; }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HotKey hk)
        {
            var r = new HotKeySection((int)hk.ModifierKeys, (int)hk.Key);
            return r;
        }
        else { return value; }
    }

    #endregion Methods
}