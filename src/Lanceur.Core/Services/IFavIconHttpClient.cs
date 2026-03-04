namespace Lanceur.Core.Services;

/// <summary>
///     Abstraction over <see cref="System.Net.Http.HttpClient" /> for sending HTTP requests
///     during favicon retrieval, enabling unit testing by decoupling the implementation
///     from the concrete HTTP client.
/// </summary>
public interface IFavIconHttpClient
{
    #region Methods

    /// <summary>
    ///     Send an HTTP request as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

    #endregion
}