using System.Text;

namespace Lanceur.Core.Managers;

public static class CmdlineManager
{
    #region Fields

    /// <summary>
    ///     The characters that can be used as a special name, and therefore as a store shortcut.
    /// </summary>
    public static IReadOnlyList<string> SpecialCharacters =>
    [
        "$",
        "&",
        "|",
        "@",
        "#",
        ")",
        "§",
        "!",
        "{",
        "}",
        "_",
        "\\",
        "+",
        "-",
        "*",
        "/",
        "=",
        "<",
        ">",
        ",",
        ";",
        ":",
        "%",
        "?",
        "."
    ];

    #endregion

    #region Methods

    public static string GetSpecialName(string cmdline)
    {
        if (cmdline.Length <= 0)
        {
            throw new ArgumentException("Cmdline should not be empty", nameof(cmdline));
        }

        var first = cmdline[0];
        var accumulator = new StringBuilder($"{cmdline[0]}");
        for (var i = 1; i < cmdline.Length; i++)
        {
            if (cmdline[i] != first) { break; }

            accumulator.Append(cmdline[i]);
        }

        return accumulator.ToString();
    }

    public static bool HasSpecialName(string cmdName) => cmdName.Length > 0
                                                         && SpecialCharacters.Contains($"{cmdName[0]}");

    /// <summary>
    ///     Determines whether <paramref name="value" /> is exactly one special character.
    ///     This is the only form a store shortcut can take.
    /// </summary>
    public static bool IsSpecialCharacter(string? value) => value is { Length: 1 } && SpecialCharacters.Contains(value);

    #endregion
}