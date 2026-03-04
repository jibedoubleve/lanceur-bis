using Humanizer;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public class FavIconHttpClient : IFavIconHttpClient
{
    #region Fields

    private static readonly HttpClient HttpClient = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = 1.Minutes()
    });

    #endregion

    #region Methods

    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) => HttpClient.SendAsync(request);

    #endregion
}