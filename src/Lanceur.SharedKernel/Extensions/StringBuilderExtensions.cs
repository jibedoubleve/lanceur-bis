using System.Text;

namespace Lanceur.SharedKernel.Extensions;

public static class StringBuilderExtensions
{
    #region Methods

    public static void AppendNewLine(this StringBuilder stringBuilder) => stringBuilder.Append(Environment.NewLine);

    #endregion Methods
}