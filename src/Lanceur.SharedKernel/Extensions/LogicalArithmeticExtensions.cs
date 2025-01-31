namespace Lanceur.SharedKernel.Extensions;

public static class LogicalArithmeticExtensions
{
    #region Methods

    /// <summary>
    ///     Checks if the AND operation between <paramref name="source" /> and <paramref name="flag" />
    ///     results in <paramref name="flag" />. This is typically used to verify if a specific flag is set.
    /// </summary>
    /// <param name="source">The source integer value containing flags.</param>
    /// <param name="flag">The flag to check against the source value.</param>
    /// <returns>True if the AND result equals the flag, otherwise false.</returns>
    public static bool IsFlagSet(this int source, int flag) => (source & flag) == flag;

    #endregion
}