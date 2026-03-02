using System.Diagnostics;
using System.IO;

namespace Lanceur.Infra.Win32.Helpers;

/// <summary>
///     Provides methods to launch Windows shell components.
/// </summary>
public static class WindowsShell
{
    #region Properties

    /// <summary>
    ///     Gets the full path to the Windows Explorer executable (<c>explorer.exe</c>).
    /// </summary>
    private static string Explorer => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "explorer.exe"
    );

    #endregion

    #region Methods

    /// <summary>
    ///     Starts a new instance of Windows Explorer with the specified arguments.
    /// </summary>
    /// <param name="args">Optional arguments passed to <c>explorer.exe</c>, such as a folder path to open.</param>
    /// <returns>The <see cref="Process" /> instance representing the started Explorer process.</returns>
    public static Process StartExplorer(params string[] args) => Process.Start(Explorer, args);

    #endregion
}