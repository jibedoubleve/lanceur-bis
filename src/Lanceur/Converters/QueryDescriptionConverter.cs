using Lanceur.Core.Formatters;
using Splat;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Lanceur.Converters
{
    public class QueryDescriptionConverter : IValueConverter
    {
        #region Fields

        private readonly IStringFormatter _formatter;

        #endregion Fields

        #region Constructors

        public QueryDescriptionConverter()
        {
            _formatter = Locator.Current.GetService<IStringFormatter>();

            ArgumentNullException.ThrowIfNull(_formatter);
        }

        public QueryDescriptionConverter(IStringFormatter formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            _formatter = formatter;
        }

        #endregion Constructors

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string description && description.StartsWith("package:"))
            {
                return "Packaged Application";
            }
            else
            {
                return _formatter.Format(value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}