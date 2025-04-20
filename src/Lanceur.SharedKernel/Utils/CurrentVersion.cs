using System.Diagnostics;
using System.Reflection;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.SharedKernel.Utils;

/// <summary>
///     Represents the current application version along with the associated Git commit.
/// </summary>
public record CurrentVersion
{
    #region Constructors

    private CurrentVersion(string version, string suffix, string commit)
    {
        Commit = commit;
        Suffix = suffix;
        Version = new(version);
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the Git commit hash associated with the current build.
    /// </summary>
    public string Commit { get; }

    public string Suffix { get; set; }

    /// <summary>
    ///     Gets the version of the current build.
    /// </summary>
    public Version Version { get; }

    #endregion

    #region Methods

    private static (string Version, string Suffix) GetVersion(string version)
    {
        if (version.IsNullOrWhiteSpace()) return ("0.0.0", string.Empty);

        var idx = version.IndexOf('-');
        if (idx <= 0) return (version, string.Empty);

        return (
            version[..idx],
            version[(idx + 1)..]
        );
    }

    /// <summary>
    ///     Creates a <see cref="CurrentVersion" /> instance from the version information embedded in a given assembly.
    /// </summary>
    /// <param name="asm">The assembly from which to extract version information.</param>
    /// <returns>A <see cref="CurrentVersion" /> instance representing the assembly's version and commit.</returns>
    public static CurrentVersion FromAssembly(Assembly asm)
    {
        var fullVer = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        return FromFullVersion(fullVer);
    }

    /// <summary>
    ///     Creates a <see cref="CurrentVersion" /> instance from a full version string that includes semantic versioning and a
    ///     commit hash.
    /// </summary>
    /// <param name="fullVersion">The full version string, typically in the format "x.y.z+commitHash".</param>
    /// <returns>A <see cref="CurrentVersion" /> instance representing the parsed version and commit.</returns>
    /// <remarks>This method exists mainly for testing purpose.</remarks>
    public static CurrentVersion FromFullVersion(string fullVersion)
    {
        var semverSplit = fullVersion?.Split(["+"], StringSplitOptions.RemoveEmptyEntries);
        var commit = semverSplit?.Length > 1 ? semverSplit[1] : string.Empty;
        (string Version, string Suffix) semVer = ("0.0.0", "");

        if (semverSplit?.Length > 0) semVer = GetVersion(semverSplit[0]);
        return new(semVer.Version, semVer.Suffix, commit);
    }

    #endregion
}