using System;
using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Converters
{
    public class QueryDescriptionConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string description && description.StartsWith("package:"))
            {
                return "Packaged Application";
            }
            else { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}