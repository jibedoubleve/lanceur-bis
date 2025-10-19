using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Extensions;

public static class LoggerExtensions
{
    public static void LogVersion(this ILogger logger, Assembly asm)
    {
        var attributes = asm.GetCustomAttributes(false);
        
        var fVer = new AssemblyFileVersionAttribute("0.0.0");
        var iVer = new AssemblyInformationalVersionAttribute("0.0.0");
        var version = asm.GetName().Version;

        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case AssemblyFileVersionAttribute fv:
                    fVer = fv; 
                    break;
                case AssemblyInformationalVersionAttribute iv:
                   iVer=iv; 
                   break;
            }
        }

        logger.LogInformation(
            """
            Version: {Version}
            InformationalVersion: {InformationalVersion}
            FileVersion: {FileVersion}
            """,
            version,
            iVer.InformationalVersion,
            fVer.Version
        );
    }
}