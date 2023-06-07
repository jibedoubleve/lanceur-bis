using Humanizer;
using Lanceur.Core.Formatters;

namespace Lanceur.Infra.Formatters
{
    public class LimitedStringLengthFormatter : IStringFormatter
    {
        #region Fields

        private const int LENGTH = 115;

        #endregion Fields

        #region Methods

        public string Format(object value) => value?.ToString()?.Truncate(LENGTH, "(...)");

        #endregion Methods
    }
}