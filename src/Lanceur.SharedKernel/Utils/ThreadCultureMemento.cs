using System.Globalization;

namespace Lanceur.SharedKernel.Utils;

public record ThreadCultureMemento
{
    #region Properties

    public CultureInfo Culture { get; init; }
    public CultureInfo UiCulture { get; init; }

    #endregion
}