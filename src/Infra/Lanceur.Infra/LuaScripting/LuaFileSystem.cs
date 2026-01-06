using System.Collections;

namespace Lanceur.Infra.LuaScripting;

/// <summary>
///     Provides LuaFileSystem (lfs) compatible API for Lua scripts
/// </summary>
public class LuaFileSystem
{
    #region Methods

    private static string GetPermissionsString(FileSystemInfo info)
    {
        var perms = "";
        perms += info.Attributes.HasFlag(FileAttributes.Directory) ? "d" : "-";
        perms += "r"; // Read permission (simplified)
        perms += info.Attributes.HasFlag(FileAttributes.ReadOnly) ? "-" : "w";
        perms += "x"; // Execute permission (simplified)
        return perms;
    }

    /// <summary>
    ///     Gets file or directory attributes
    /// </summary>
    /// <param name="path">Path to file or directory</param>
    /// <param name="attributeName">Optional specific attribute name to return</param>
    /// <returns>Table with attributes or specific attribute value</returns>
    public object Attributes(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            throw new FileNotFoundException($"No such file or directory: {path}");

        var isDirectory = Directory.Exists(path);
        var info = isDirectory ? (FileSystemInfo)new DirectoryInfo(path) : new FileInfo(path);

        var attributes = new Dictionary<string, object>
        {
            ["mode"] = isDirectory ? "directory" : "file",
            ["dev"] = 0, // Not applicable on Windows
            ["ino"] = 0, // Not applicable on Windows
            ["nlink"] = 1,
            ["uid"] = 0, // Not applicable on Windows
            ["gid"] = 0, // Not applicable on Windows
            ["rdev"] = 0,
            ["access"] = new DateTimeOffset(info.LastAccessTime).ToUnixTimeSeconds(),
            ["modification"] = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds(),
            ["change"] = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds(),
            ["size"] = isDirectory ? 0 : ((FileInfo)info).Length,
            ["permissions"] = GetPermissionsString(info),
            ["blocks"] = 0,
            ["blksize"] = 0
        };

        return attributes;
    }

    /// <summary>
    ///     Changes the current working directory
    /// </summary>
    /// <param name="path">Path to new working directory</param>
    /// <returns>true on success</returns>
    public bool Chdir(string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"No such directory: {path}");

        Directory.SetCurrentDirectory(path);
        return true;
    }

    /// <summary>
    ///     Gets the current working directory
    /// </summary>
    /// <returns>Current working directory path</returns>
    public string Currentdir() => Directory.GetCurrentDirectory();

    /// <summary>
    ///     Creates an iterator function for directory entries
    /// </summary>
    /// <param name="path">Directory path to iterate</param>
    /// <returns>IEnumerable of directory entries</returns>
    public IEnumerable Dir(string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"No such directory: {path}");

        var entries = new List<string> { ".", ".." };
        entries.AddRange(Directory.GetFileSystemEntries(path).Select(Path.GetFileName)!);
        return entries;
    }

    /// <summary>
    ///     Determines whether the specified path exists as either a file or a directory.
    /// </summary>
    /// <param name="path">The path to check for existence.</param>
    /// <returns>
    ///     <see langword="true" /> if the path exists as a file or directory; otherwise, <see langword="false" />.
    /// </returns>
    public bool Exists(string path) => Path.Exists(path);
    
    /// <summary>
    ///     Creates a new directory
    /// </summary>
    /// <param name="dirname">Directory name/path to create</param>
    /// <returns>true on success</returns>
    public bool Mkdir(string dirname)
    {
        Directory.CreateDirectory(dirname);
        return true;
    }

    /// <summary>
    ///     Removes a directory
    /// </summary>
    /// <param name="dirname">Directory name/path to remove</param>
    /// <returns>true on success</returns>
    public bool Rmdir(string dirname)
    {
        if (!Directory.Exists(dirname)) throw new DirectoryNotFoundException($"No such directory: {dirname}");

        Directory.Delete(dirname, false);
        return true;
    }

    #endregion
}