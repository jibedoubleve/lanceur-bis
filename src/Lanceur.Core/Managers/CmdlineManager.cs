using Lanceur.Core.Models;

namespace Lanceur.Core.Managers;

public static class CmdlineManager
{
    #region Fields

    private static readonly string[] SpecialNames = ["$", "&", "|", "@", "#", "(", ")", "§", "!", "{", "}", "_", "\\", "+", "-", "*", "/", "=", "<", ">", ",", ";", ":", "%", "?", "."];

    #endregion

    #region Methods

    private static string GetSpecialName(string cmdline)
    {
        return cmdline.Length switch
        {
            > 1 when cmdline[0] == cmdline[1] => cmdline[..2],
            > 0                               => cmdline[..1],
            _                                 => throw new ArgumentException($"Cmdline is too short (length {cmdline.Length})")
        };
    }

    private static bool HasSpecialName(string cmdName)
    {
        cmdName = cmdName.Trim();
        var res1 = false;
        var res2 = false;

        if (cmdName.Length > 0) res1 = SpecialNames.Contains(cmdName[..1]);
        if (cmdName.Length > 1) res2 = SpecialNames.Contains(cmdName[..2]);

        return res1 || res2;
    }

    public static Cmdline BuildFromText(string cmdline)
    {
        cmdline = cmdline?.Trim();
        var name = string.Empty;
        var args = string.Empty;
        cmdline ??= string.Empty;

        if (HasSpecialName(cmdline))
        {
            name = GetSpecialName(cmdline);
            args = cmdline[name.Length..];
        }
        else
        {
            var elements = cmdline.Split(" ");
            if (elements.Length > 0)
            {
                name = elements[0];
                args = cmdline[name.Length..];
            }
        }

        return new(name, args.Trim());
    }

    public static Cmdline CloneWithNewParameters(string newParameters, Cmdline cmd) => BuildFromText($"{cmd?.Name} {newParameters}");

    #endregion
}