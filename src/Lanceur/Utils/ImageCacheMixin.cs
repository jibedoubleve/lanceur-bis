using Lanceur.Infra.Constants;
using System;
using System.Linq;

namespace Lanceur.Utils
{
    public static class ImageCacheMixin
    {
        #region Fields

        private static readonly string[] SupportedSchemes = { "http", "https" };

        #endregion Fields

        #region Methods

        public static bool IsUrl(this string address)
        {
            var result = Uri.TryCreate(address, new UriCreationOptions(), out var uri)
                         && SupportedSchemes.Contains(uri.Scheme);
            return result;

        }
        public static string GetKeyForFavIcon(this string address)
        {
            ArgumentNullException.ThrowIfNull(address);
            if (!Uri.TryCreate(address, new UriCreationOptions(), out _))
                throw new NotSupportedException("The specified address is not a valid URL");

            return $"{AppPaths.FaviconPrefix}{new Uri(address).Host}";
        }
        #endregion Methods
    }
}