using System.Net;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Web;

public class FavIconDownloader : IFavIconDownloader
{
    #region Fields

    private readonly HttpClient _client = new();

    /// <summary>
    ///     Cache the path where it is impossible to retrieve the favicon to not spend time
    ///     on something that doesn't work
    /// </summary>
    private readonly HashSet<string> _failedPaths = [];

    private readonly ILogger<FavIconDownloader> _logger;

    private static readonly Dictionary<string, (bool IsManual, string Url)> FaviconUrls = new()
    {
        ["DuckDuckGo"] = (IsManual: false, Url: "https://icons.duckduckgo.com/ip2/{0}.ico"),
        ["Google"] = (IsManual: false, Url: "https://www.google.com/s2/favicons?domain_url={0}"),
        ["Manual (png)"] = (IsManual: true, Url: "favicon.png"),
        ["Manual (ico)"] = (IsManual: true, Url: "favicon.ico")
    };

    #endregion

    #region Constructors

    public FavIconDownloader(ILogger<FavIconDownloader> logger) => _logger = logger;

    #endregion

    #region Methods

    private string  GetFaviconUrl(Uri url, KeyValuePair<string, (bool IsManual, string Url)> manager)
    {
        var requestUrl = manager.Value.IsManual
            ? new Uri(url, manager.Value.Url).ToString()
            : manager.Value.Url.Format(url.Host);

        _logger.LogTrace("Request Url: {Url}", requestUrl);
        return requestUrl;
    }

    private async Task<bool> SaveThumbnailAsync(Uri favIconUrl, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(favIconUrl);
        ArgumentNullException.ThrowIfNull(outputPath);

        var foundFavIcon = false;

        try
        {
            var bytes = await _client.GetByteArrayAsync(favIconUrl);
            if (bytes.Length == 0)
            {
                _logger.LogWarning(
                    "Failed to save favicon to {OutputPath}. Favicon url: {FavIconUrl}",
                    outputPath,
                    favIconUrl
                );
                return false;
            }

            await File.WriteAllBytesAsync(outputPath, bytes);
            foundFavIcon = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(
                ex,
                "Failed to retrieve favicon for {Url} with {FavIconUrl}",
                favIconUrl,
                favIconUrl
            );
        }

        return foundFavIcon;
    }

    public async Task<bool> RetrieveAndSaveFavicon(Uri url, string outputPath)
    {
        if (_failedPaths.Contains(url.ToString()))
        {
            _logger.LogTrace("{Url} is in the failed paths for favicon retrieving.", url);
            return false;
        }

        foreach (var faviconUrl in FaviconUrls)
            try
            {
                var requestUrl = GetFaviconUrl(url, faviconUrl);
                var result = await _client.SendAsync(new(HttpMethod.Get, requestUrl));

                _logger.LogTrace(
                    "Checking favicon with {FavIconManager} - Status: {Status} - Host: {Host}",
                    faviconUrl.Key,
                    result.StatusCode,
                    url.Host
                );
                if (result.StatusCode != HttpStatusCode.OK) continue;

                if (!await SaveThumbnailAsync(new(requestUrl), outputPath)) continue;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(
                    ex,
                    "Error while retrieving favicon for {Url} - {Manager}",
                    url,
                    faviconUrl.Value.Url
                );
            }

        _failedPaths.Add(url.ToString());
        return false;
    }

    #endregion
}