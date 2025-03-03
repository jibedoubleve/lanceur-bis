using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public record Cmdline
{
    #region Constructors

    public Cmdline(string name, string parameters = "")
    {
        Name = (name ?? "").Trim();
        Parameters = (parameters ?? "").Trim();

        if (Name.Contains(' ')) throw new ArgumentException("The name of a cmdline cannot contain whitespaces.", nameof(name));
    }

    #endregion

    #region Properties

    private bool HasParameters => !Parameters.IsNullOrEmpty();
    public static Cmdline Empty => new(string.Empty, string.Empty);
    public bool IsEmpty => Name.IsNullOrEmpty() && !HasParameters;
    public string Name { get; init; }
    public string Parameters { get; }

    #endregion

    #region Methods

    public static Cmdline CloneWithNewParameters(string newParameters, Cmdline cmd) => Parse($"{cmd?.Name} {newParameters}");

    public bool IsNullOrEmpty() => Name.IsNullOrWhiteSpace();

    /// <summary>
    ///     Implicitly converts a <see cref="Cmdline" /> object to a <see cref="string" /> by invoking its
    ///     <see cref="ToString" /> method.
    /// </summary>
    /// <param name="source">The <see cref="Cmdline" /> object to be converted to a string.</param>
    /// <returns>A <see cref="string" /> representation of the <paramref name="source" />.</returns>
    public static implicit operator string(Cmdline source) => source.ToString();

    public static Cmdline Parse(string cmdline)
    {
        cmdline = (cmdline ?? string.Empty).Trim();

        if (CmdlineManager.HasSpecialName(cmdline))
        {
            return new(
                CmdlineManager.GetSpecialName(cmdline),
                cmdline[1..]
            );
        }

        var elements = cmdline.Split(" ");
        if (elements.Length <= 0) return Empty;

        var name = elements[0];
        return new(
            name,
            cmdline[name.Length..]);
    }

    public override string ToString() => $"{Name ?? string.Empty} {Parameters ?? string.Empty}".Trim();

    #endregion
}