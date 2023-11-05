using System.Text.Json;

namespace Lanceur.SharedKernel.Mixins;

public static class CloningMixin
{
    #region Methods

    /// <summary>
    /// Clone object by serialising/deserialising it
    /// </summary>
    /// <param name="obj">The object to clone</param>
    /// <typeparam name="T">The type of the object to clone</typeparam>
    /// <returns>A new instance of the object with the same state</returns>
    public static T CloneObject<T>(this T obj)
        where T: new()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return JsonSerializer.Deserialize<T>(bytes);
    }

    #endregion Methods
}