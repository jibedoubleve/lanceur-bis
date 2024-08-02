namespace Lanceur.SharedKernel.Utils;

public class FileManager
{
    #region Methods

    public static string[] FindWithExtension(string root, string extension)
    {
        extension = "." + extension.TrimStart('.').ToLower();
        var result = new List<string>();
        if (Directory.Exists(root))
        {
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                if (Path.GetExtension(file) == extension)
                    result.Add(file);
            return result.ToArray();
        }
        else { throw new NotSupportedException($"The path '{root}' does not exist."); }
    }

    #endregion Methods
}