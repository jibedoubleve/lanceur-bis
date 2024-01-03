using Lanceur.Core.Formatters;
using Lanceur.Infra.Formatters;
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

        public QueryDescriptionConverter() : this(null)
        {
        }

        public QueryDescriptionConverter(IStringFormatter formatter, IReadonlyDependencyResolver locator = null)
        {
            var l = locator ?? Locator.Current;
            _formatter = formatter ?? l.GetService<IStringFormatter>();
            ArgumentNullException.ThrowIfNull(_formatter);
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
                var msg = _formatter.Format(value);
                return new LimitedStringLengthFormatter().Format(msg);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}