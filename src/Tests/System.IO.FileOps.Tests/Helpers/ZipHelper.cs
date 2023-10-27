using System.IO.Compression;

namespace System.IO.FileOps.Test.Helpers;

public class ZipHelper
{
    #region Methods

    /// <summary>
    ///     Zip the specified text file into the specified zip file
    /// </summary>
    /// <param name="textFile">A random text file to create in the zip</param>
    /// <param name="zipFile">The resulting zip file</param>
    public static void Zip(string textFile, string zipFile)
    {
        var sourceDir = new FileInfo(textFile).Directory?.FullName;

        if (sourceDir is null) throw new FileNotFoundException($"The file '{textFile}' does not exist.");

        using var fileStream = File.Create(textFile);
        using var writer = new StreamWriter(fileStream);
        {
            writer.WriteLine("some random text");
            writer.Flush();
            writer.Close();
        }

        ZipFile.CreateFromDirectory(sourceDir, zipFile);
    }

    #endregion Methods
}