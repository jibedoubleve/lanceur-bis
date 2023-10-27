using System;
using System.Globalization;
using System.Windows.Data;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Converters
{
    public class StringToRunAsConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RunAs startMode)
            {
                return startMode switch
                {
                    RunAs.Admin => "Admin",
                    RunAs.CurrentUser => "CurrentUser",
                    _ => null,
                };
            }
            else { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    "Admin" => RunAs.Admin,
                    "CurrentUser" => RunAs.CurrentUser,
                    _ => null,
                };
            }
            else { return null; }
        }

        #endregion Methods
    }
}