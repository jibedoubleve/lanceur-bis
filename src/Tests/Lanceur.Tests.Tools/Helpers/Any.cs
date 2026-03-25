using ScottPlot;

namespace Lanceur.Tests.Tools.Helpers;

/// <summary>
///     Provides factory methods for generating arbitrary test values.
///     Use this class in tests when the exact value doesn't matter,
///     only its type or size.
/// </summary>
public static class Any
{
    #region Methods

    /// <summary>
    ///     Generates a random string of the specified length.
    /// </summary>
    /// <param name="size">The number of characters in the generated string.</param>
    /// <returns>A random string of the given length.</returns>
    public static string String(int size) => Generate.RandomString(size);

    /// <summary>
    ///     Generates a random file name suitable for use as a path in tests.
    /// </summary>
    /// <returns>A random file name string.</returns>
    public static string AbsolutePath() => Path.GetRandomFileName();

    #endregion
}