namespace Lanceur.SharedKernel.Extensions;

public static class UriExtensions
{
    #region Methods

    private static Uri ToUri(this string path, UriKind kind) => new(path, kind);

    public static Uri GetAuthority(this Uri baseUri) => new(baseUri.GetLeftPart(UriPartial.Authority));

    public static IEnumerable<Uri> GetFavicons(this Uri baseUri)
    {
        var uri = baseUri.GetAuthority();
        yield return new(uri, "favicon.ico");
        yield return new(uri, "favicon.png");
    }

    public static Uri ToUriRelative(this string path) => path.ToUri(UriKind.Relative);

    #endregion
}