using System.Net;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Web
{
    ///<inheritdoc />
    public class FavIconDownloader : IFavIconDownloader
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _failedPaths = new();

        public FavIconDownloader(ILogger logger)
        {
            _logger = logger;
        }
        #region Methods

        public async Task<bool> CheckExistsAsync(Uri url)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.SendAsync(new(HttpMethod.Head, url));
                return result.StatusCode == HttpStatusCode.OK;
            }
            catch { return false; }
        }

        ///<inheritdoc />
        public async Task<bool> SaveToFileAsync(Uri url, string path)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(url);
                ArgumentNullException.ThrowIfNull(path);

                if (_failedPaths.Contains(path)) return false;
                if (File.Exists(path)) return true;
                
                var uri = url.GetFavicon();
                
                var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(uri);
                if (bytes.Length == 0) return true;

                await File.WriteAllBytesAsync(path, bytes);
                return true;
            }
            catch (Exception ex)
            {
                _failedPaths.Add(path);
                _logger.LogInformation(ex, "An error occured while saving FavIcon {Path}", path);
                return false;
            }
        }

        #endregion Methods
    }
}