using Humanizer;
using Lanceur.Core.Formatters;

namespace Lanceur.Infra.Formatters;

public class LimitedStringLengthFormatter : IStringFormatter
{
    #region Fields

    private const int MaxLength = 90;

    #endregion Fields

    #region Methods

    public string Format(object value) => value?.ToString()?.Truncate(MaxLength, "(...)");

    #endregion Methods
}