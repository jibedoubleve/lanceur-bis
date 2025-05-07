namespace Lanceur.Core.Managers;

public static class CmdlineManager
{
    #region Fields

    private static readonly string[] SpecialNames = ["$", "&", "|", "@", "#", ")", "§", "!", "{", "}", "_", "\\", "+", "-", "*", "/", "=", "<", ">", ",", ";", ":", "%", "?", "."];

    #endregion

    #region Methods

    public static string GetSpecialName(string cmdline)
    {
        return cmdline.Length switch
        {
            > 1 when cmdline[0] == cmdline[1] => cmdline[..2],
            > 0                               => cmdline[..1],
            _                                 => throw new ArgumentException($"Cmdline is too short (length {cmdline.Length})")
        };
    }

    public static bool HasSpecialName(string cmdName)
    {
        cmdName = cmdName.Trim();
        var res1 = false;
        var res2 = false;

        if (cmdName.Length > 0) res1 = SpecialNames.Contains(cmdName[..1]);
        if (cmdName.Length > 1) res2 = SpecialNames.Contains(cmdName[..2]);

        return res1 || res2;
    }

    #endregion
}