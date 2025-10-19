using System.Text.Json;

namespace Lanceur.SharedKernel.Extensions;

public static class JsonExtensions
{
    #region Fields

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    #endregion

    #region Methods

    public static string ToJson(this object obj) => JsonSerializer.Serialize(obj, Options);

    #endregion
}