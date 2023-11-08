using System.Text;

namespace Lanceur.SharedKernel.Mixins;

public static class StringBuilderMixin
{
    #region Methods

    public static void AppendNewLine(this StringBuilder stringBuilder) => stringBuilder.Append(Environment.NewLine);

    #endregion Methods
}