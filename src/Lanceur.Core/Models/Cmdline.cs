using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public record Cmdline
{
    #region Constructors

    public Cmdline(string name, string parameters = "")
    {
        name ??= string.Empty;
        
        if (name.Contains(' ')) throw new ArgumentException("The name of a cmdline cannot contain whitespaces.", nameof(name));

        name = name.Trim();
        
        Name = name;
        Parameters = parameters ?? string.Empty;
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

    public static Cmdline BuildFromText(string commandline) => CmdlineManager.BuildFromText(commandline);

    public bool IsNullOrEmpty() => Name.IsNullOrWhiteSpace();

    public override string ToString() => $"{Name ?? string.Empty} {Parameters ?? string.Empty}".Trim();

    /// <summary>
    /// Implicitly converts a <see cref="Cmdline"/> object to a <see cref="string"/> by invoking its <see cref="ToString"/> method.
    /// </summary>
    /// <param name="source">The <see cref="Cmdline"/> object to be converted to a string.</param>
    /// <returns>A <see cref="string"/> representation of the <paramref name="source"/>.</returns>
    public static implicit operator string(Cmdline source) => source.ToString();

    #endregion
}