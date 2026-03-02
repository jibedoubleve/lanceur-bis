using System.Text.Json;

namespace Lanceur.SharedKernel.Extensions;

public static class CloningExtensions
{
    #region Methods

    /// <summary>
    ///     Creates a deep copy of the specified object by serializing and deserializing it.
    /// </summary>
    /// <param name="obj">The object to be cloned.</param>
    /// <typeparam name="T">The type of the object to be cloned.</typeparam>
    /// <returns>A new instance of <typeparamref name="T" /> with the same state as the original object.</returns>
    public static T CloneObject<T>(this T obj)
        where T : new()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return JsonSerializer.Deserialize<T>(bytes);
    }

    #endregion
}