using Lanceur.Infra.Constants;

namespace Lanceur.Infra.Win32.Thumbnails
{
    internal static class ImageRepositoryMixin
    {
        #region Fields

        private static readonly string[] SupportedSchemes = { "http", "https" };

        #endregion Fields

        #region Methods

        public static string GetKeyForFavIcon(this string address)
        {
            ArgumentNullException.ThrowIfNull(address);
            return Uri.TryCreate(address, new UriCreationOptions(), out _)
                ? $"{Paths.FaviconPrefix}{new Uri(address).Host}"
                : string.Empty;
        }

        public static bool IsUrl(this string address)
        {
            var result = Uri.TryCreate(address, new UriCreationOptions(), out var uri)
                         && SupportedSchemes.Contains(uri.Scheme);
            return result;
        }

        #endregion Methods
    }
}