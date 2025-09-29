namespace Lanceur.SharedKernel.Extensions;

public static class TimeSpanExtensions
{
    #region Methods

    public static string ToStringInSeconds(this TimeSpan timeSpan) => $"{timeSpan.TotalSeconds:F3}";

    #endregion
}