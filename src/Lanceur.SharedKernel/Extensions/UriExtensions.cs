namespace Lanceur.SharedKernel.Extensions;

public static class UriExtensions
{
    #region Methods

    private static Uri ToUri(this string path, UriKind kind) => new(path, kind);

    /// <summary>
    ///     Returns a <see cref="Uri" /> containing only the scheme, host, and port of the specified URI.
    ///     Returns <c>null</c> if the authority part cannot be determined.
    /// </summary>
    /// <param name="baseUri">The URI from which to extract the authority.</param>
    /// <returns>
    ///     A <see cref="Uri" /> representing the authority (e.g. <c>https://example.com</c>),
    ///     or <c>null</c> if the authority part is empty.
    /// </returns>
    public static Uri? GetAuthority(this Uri baseUri)
    {
        var leftPart = baseUri.GetLeftPart(UriPartial.Authority);
        return leftPart.IsNullOrEmpty()
            ? null
            : new Uri(leftPart);
    }

    public static Uri ToUriRelative(this string path) => path.ToUri(UriKind.Relative);

    #endregion
}