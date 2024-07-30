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

        ///<inheritdoc />
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
        public async Task<bool> SaveToFileAsync(Uri url, string outputPath)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(url);
                ArgumentNullException.ThrowIfNull(outputPath);

                if (_failedPaths.Contains(outputPath)) return false;
                if (File.Exists(outputPath)) return true;
                
                var uris = url.GetFavicons();
                var httpClient = new HttpClient();

                foreach (var uri in uris)
                {
                    _logger.LogTrace("Checking favicon url {Url}", uri);
                    if (! await CheckExistsAsync(uri)) continue;
                    
                    var bytes = await httpClient.GetByteArrayAsync(uri);
                    if (bytes.Length == 0) continue;
                    
                    await File.WriteAllBytesAsync(outputPath, bytes);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _failedPaths.Add(outputPath);
                _logger.LogInformation(ex, "An error occured while saving FavIcon {Path}", outputPath);
                return false;
            }
        }

        #endregion Methods
    }
}