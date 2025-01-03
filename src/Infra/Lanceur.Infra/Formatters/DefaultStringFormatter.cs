using Lanceur.Core.Formatters;

namespace Lanceur.Infra.Formatters;

public class DefaultStringFormatter : IStringFormatter
{
    #region Methods

    public string Format(object value) => value?.ToString();

    #endregion Methods
}