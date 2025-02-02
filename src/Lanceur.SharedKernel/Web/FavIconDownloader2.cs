using System.Net;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Web;

public class FavIconDownloader2 : IFavIconDownloader
{
    #region Fields

    private readonly HttpClient _client = new();

    /// <summary>
    ///     Cache the path where it is impossible to retrieve the favicon to not spend time
    ///     on something that doesn't work
    /// </summary>
    private readonly HashSet<string> _failedPaths = [];

    private readonly ILogger<FavIconDownloader2> _logger;

    private static readonly Dictionary<string, string> FavIconManagers = new()
    {
        ["DuckDuckGo"] = "https://icons.duckduckgo.com/ip2/{0}.ico",
        ["Google"] = "https://www.google.com/s2/favicons?domain_url={0}",
    };

    #endregion

    #region Constructors

    public FavIconDownloader2(ILogger<FavIconDownloader2> logger) => _logger = logger;

    #endregion

    #region Methods

    private string  RequestUrl(Uri url, string manager)
    {
        var requestUrl = manager.Format(url.Host);
        _logger.LogTrace("Request Url: {Url}", requestUrl);
        return requestUrl;
    }

    public async Task<bool> CheckExistsAsync(Uri url)
    {
        try
        {
            foreach (var manager in FavIconManagers)
            {
                var result = await _client.SendAsync(new(HttpMethod.Get, RequestUrl(url, manager.Value)));
                _logger.LogTrace(
                    "Checking favicon with {FavIconManager} - Status: {Status} - Host: {Host}",
                    manager.Key,
                    result.StatusCode,
                    url.Host
                );

                if (result.StatusCode == HttpStatusCode.OK) return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _failedPaths.Add(url.AbsoluteUri);
            _logger.LogWarning(ex, "Failed to check favicon with {Url}", url);
        }

        return false;
    }

    public async Task<bool> SaveToFileAsync(Uri url, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(outputPath);

        if (_failedPaths.TryGetValue(url.AbsoluteUri, out _)) return false;
        
        var tasks = new List<Task>();
        var foundFavIcon = false;

        foreach (var favIconUrl in FavIconManagers.Select(manager => manager.Value.Format(url.Host)))
        {
            var bytes = await _client.GetByteArrayAsync(favIconUrl);
            if (bytes.Length == 0)
            {
                _logger.LogWarning("Failed to save favicon to {Url}", url);
                continue;
            }

            tasks.Add(File.WriteAllBytesAsync(outputPath, bytes));
            foundFavIcon = true;
            break;
        }

        await Task.WhenAll(tasks);
        return foundFavIcon;
    }

    #endregion
}