namespace Lanceur.Tests.Tools.Generators;

public static class Generate
{
    #region Methods

    public static string Text(uint? length = null) =>
        length == null 
            ? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString()[..8];

    public static string FilName() => Path.GetRandomFileName();

    #endregion
}