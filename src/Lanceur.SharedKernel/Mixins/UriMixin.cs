namespace Lanceur.SharedKernel.Mixins;

public static class UriMixin
{
    #region Methods

    public static Uri ToUri(this string path, UriKind kind) => new(path, kind);

    public static Uri ToUriAbsolute(this string path) => path.ToUri(UriKind.Absolute);

    public static Uri ToUriRelative(this string path) => path.ToUri(UriKind.Relative);

    #endregion Methods
}