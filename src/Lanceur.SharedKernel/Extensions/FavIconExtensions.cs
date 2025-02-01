namespace Lanceur.SharedKernel.Extensions;

public static class FavIconExtensions
{
    #region Fields

    private static readonly string[] SupportedSchemes = { "http", "https" };
    public const string FilePrefix = "favicon_";

    #endregion Fields

    #region Methods

    public static string GetKeyForFavIcon(this string address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return Uri.TryCreate(address, new UriCreationOptions(), out _)
            ? $"{FilePrefix}{new Uri(address).Host}"
            : string.Empty;
    }

    public static bool IsUrl(this string address)
    {
        var result = Uri.TryCreate(address, new UriCreationOptions(), out var uri) && SupportedSchemes.Contains(uri.Scheme);
        return result;
    }

    #endregion Methods
}