using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public sealed record Cmdline
{
    #region Constructors

    public Cmdline(string name, string parameters = "") : this(name, parameters, null) { }

    private Cmdline(string name, string parameters, string? raw)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(parameters);

        Name = name.Trim();
        RawParameters = parameters.TrimStart();
        Parameters = parameters.Trim();
        Raw = raw ?? ToString();

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

    /// <summary>
    ///     Gets the original, unparsed string this <see cref="Cmdline" /> was built from, whitespace included.
    ///     If no raw string was provided, it is rebuilt from <see cref="Name" /> and <see cref="Parameters" />.
    /// </summary>
    public string Raw { get; }

    /// <summary>
    ///     Gets the parameters as typed, with only the leading whitespace removed.
    ///     Unlike <see cref="Parameters" />, trailing whitespace is preserved.
    /// </summary>
    public string RawParameters { get; }

    #endregion

    #region Methods

    public bool IsNullOrEmpty() => Name.IsNullOrWhiteSpace();

    public static Cmdline Parse(string? cmdline)
    {
        var raw = cmdline ?? string.Empty;
        var trimmed = raw.TrimStart();

        if (CmdlineManager.HasSpecialName(trimmed))
        {
            var name = CmdlineManager.GetSpecialName(trimmed); 
            return new Cmdline(
                name,
                trimmed[name.Length..],
                raw
            );
        }

        var elements = trimmed.Split(" ");
        if (elements.Length <= 0) { return Empty; }
        
        return new Cmdline(
            elements[0],
            trimmed[elements[0].Length..],
            raw
        );
    }

    public override string ToString() => $"{Name} {Parameters}".Trim();

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