using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

/// <summary>
///     Gets a value based on the compilation symbol.
/// </summary>
/// <returns>
///     Returns a different value depending on whether the DEBUG symbol is defined.
/// </returns>
/// <typeparam name="T">The type of the value to return</typeparam>
public class Conditional<T>
{
    #region Fields

    private readonly T _value;

    #endregion

    #region Constructors

    /// <summary>
    ///     Create an instance of this object
    /// </summary>
    /// <param name="onDebug">If <c>DEBUG</c> constant is defined then return this value</param>
    /// <param name="onRelease">Otherwise return this value.</param>
    public Conditional(T onDebug, T onRelease)
    {
        ArgumentNullException.ThrowIfNull(onDebug);
        ArgumentNullException.ThrowIfNull(onRelease);

        _value = onRelease;
        GetConditional(ref _value, onDebug);
    }

    #endregion

    #region Properties

    public T Value => _value;

    #endregion

    #region Methods

    [Conditional("DEBUG")]
    private static void GetConditional(ref T result, T onDebug)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        result = onDebug;
    }

    public static implicit operator T(Conditional<T> src) => src.Value;

    #endregion
}