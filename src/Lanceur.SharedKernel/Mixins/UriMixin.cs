namespace Lanceur.SharedKernel.Mixins;

public static class UriMixin
{
    #region Methods

    public static Uri ToUri(this string path, UriKind kind) => new(path, kind);

    public static Uri ToUriAbsolute(this string path) => path.ToUri(UriKind.Absolute);

    public static Uri ToUriRelative(this string path) => path.ToUri(UriKind.Relative);

    public static Uri GetAuthority(this Uri baseUri) => new(baseUri.GetLeftPart(UriPartial.Authority));
    
    public static Uri GetFavicon(this Uri baseUri )
    {
        var uri = baseUri.GetAuthority();
        return new(uri, "favicon.ico");
    }

    #endregion Methods
}