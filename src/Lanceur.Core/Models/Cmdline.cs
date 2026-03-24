using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public record Cmdline
{
    #region Constructors

    public Cmdline(string name, string parameters = "")
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(parameters);

        Name = name.Trim();
        Parameters = parameters.Trim();

        if (Name.Contains(' '))
        {
            throw new ArgumentException("The name of a cmdline cannot contain whitespaces.", nameof(name));
        }
    }

    #endregion

    #region Properties

    public static Cmdline Empty => new(string.Empty, string.Empty);

    public bool HasParameters => !Parameters.IsNullOrEmpty();

    public string Name { get; }
    public string Parameters { get; }

    #endregion

    #region Methods

    public bool IsNullOrEmpty() => Name.IsNullOrWhiteSpace();

    /// <summary>
    ///     Implicitly converts a <see cref="Cmdline" /> object to a <see cref="string" /> by invoking its
    ///     <see cref="ToString" /> method.
    /// </summary>
    /// <param name="source">The <see cref="Cmdline" /> object to be converted to a string.</param>
    /// <returns>A <see cref="string" /> representation of the <paramref name="source" />.</returns>
    public static implicit operator string(Cmdline source) => source.ToString();

    /// <summary>
    ///     Implicitly converts a <see cref="string" /> to a <see cref="Cmdline" /> by invoking
    ///     <see cref="Parse" />.
    /// </summary>
    /// <param name="source">The string to parse into a <see cref="Cmdline" />.</param>
    /// <returns>A <see cref="Cmdline" /> parsed from <paramref name="source" />.</returns>
    public static implicit operator Cmdline(string source) => Parse(source);

    public static Cmdline Parse(string? cmdline)
    {
        cmdline = (cmdline ?? string.Empty).Trim();

        if (CmdlineManager.HasSpecialName(cmdline))
        {
            return new Cmdline(
                CmdlineManager.GetSpecialName(cmdline),
                cmdline[1..]
            );
        }

        var elements = cmdline.Split(" ");
        if (elements.Length <= 0) { return Empty; }

        var name = elements[0];
        return new Cmdline(
            name,
            cmdline[name.Length..]
        );
    }

    public override string ToString() => $"{Name} {Parameters}".Trim();

    #endregion
}

public static class CmdLineExtension
{
    #region Methods

    /// <summary>
    ///     Determines whether the specified <see cref="Cmdline" /> is empty or null.
    /// </summary>
    /// <param name="cmdline">The <see cref="Cmdline" /> instance to check.</param>
    /// <returns>
    ///     <c>true</c> if <paramref name="cmdline" /> is null or its string representation is empty; otherwise,
    ///     <c>false</c>.
    /// </returns>
    public static bool IsEmpty(this Cmdline? cmdline)
        => cmdline is null || string.IsNullOrEmpty(cmdline);

    #endregion
}