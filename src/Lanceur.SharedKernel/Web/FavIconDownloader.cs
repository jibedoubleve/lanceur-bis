using System.Net;

namespace Lanceur.SharedKernel.Web
{
    ///<inheritdoc />
    public class FavIconDownloader : IFavIconDownloader
    {
        #region Methods

        ///<inheritdoc />
        public async Task SaveToFileAsync(Uri url, string path)
        {
            ArgumentNullException.ThrowIfNull(url);
            ArgumentNullException.ThrowIfNull(path);

            if (File.Exists(path)) return; 
            var uri = new Uri($"{url.Scheme}://{url.Host}/favicon.ico");
            var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(uri);
            if (bytes.Length == 0) return;

            await File.WriteAllBytesAsync(path, bytes);
        }

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

        #endregion Methods
    }
}