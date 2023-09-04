namespace Lanceur.SharedKernel.Mixins;

public static class FileHelper
{
    #region Methods

    public static string GetRandomTempFile(string extension = ".tmp")
    {
        extension = extension?.TrimStart('.') ?? "tmp";

        var path = Path.GetTempPath();
        var fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), extension);
        return Path.Combine(path, fileName);
    }

    #endregion Methods
}