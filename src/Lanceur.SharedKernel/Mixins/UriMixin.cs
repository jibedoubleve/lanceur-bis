namespace Lanceur.SharedKernel.Mixins;

public static class UriMixin
{
    #region Methods

    private static Uri GetAuthority(this Uri baseUri) => new(baseUri.GetLeftPart(UriPartial.Authority));

    private static Uri ToUri(this string path, UriKind kind) => new(path, kind);

    public static Uri GetFavicon(this Uri baseUri)
    {
        var uri = baseUri.GetAuthority();
        return new(uri, "favicon.ico");
    }

    public static Uri ToUriRelative(this string path) => path.ToUri(UriKind.Relative);

    #endregion Methods
}