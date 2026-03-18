using Humanizer;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public sealed class FavIconHttpClient : IFavIconHttpClient
{
    #region Fields

    private static readonly SocketsHttpHandler SocketsHttpHandler =
        new() { PooledConnectionIdleTimeout = 30.Seconds() };

    private static readonly HttpClient HttpClient = new(SocketsHttpHandler) { Timeout = 5.Seconds() };

    #endregion

    #region Methods

    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => HttpClient.SendAsync(request, cancellationToken);

    #endregion
}