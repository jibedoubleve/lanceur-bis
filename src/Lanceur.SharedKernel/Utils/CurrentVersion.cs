using System.Diagnostics;
using System.Reflection;

namespace Lanceur.SharedKernel.Utils;

public record CurrentVersion
{
    #region Constructors

    private CurrentVersion(string version, string commit)
    {
        Commit = commit;
        Version = new(version);
    }

    #endregion

    #region Properties

    public string Commit { get;  }
    public  Version Version { get; }

    #endregion

    #region Methods

    public static CurrentVersion FromAssembly(Assembly asm)
    {
        var fullVer = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        var semverSplit = fullVer?.Split(["+"], StringSplitOptions.RemoveEmptyEntries);

        var version = semverSplit?.Length > 0 ? semverSplit[0] : fullVer;
        var commit = semverSplit?.Length > 0 ? semverSplit[1] : string.Empty;
        return new(version, commit);
    }

    #endregion
}