using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Models;

public record Cmdline
{
    #region Constructors

    public Cmdline(string name, string parameters = "")
    {
        Name = name ?? string.Empty;
        Parameters = parameters ?? string.Empty;
    }

    #endregion

    #region Properties

    private bool HasParameters => !Parameters.IsNullOrEmpty();

    public static Cmdline Empty => new(string.Empty, string.Empty);
    public bool IsEmpty => Name.IsNullOrEmpty() && !HasParameters;
    public string Name { get; }
    public string Parameters { get; }

    #endregion

    #region Methods

    public static Cmdline BuildFromText(string commandline) => CmdlineManager.BuildFromText(commandline);

    public bool IsNullOrEmpty() => Name.IsNullOrWhiteSpace();

    public override string ToString() => $"{Name ?? string.Empty} {Parameters ?? string.Empty}".Trim();

    #endregion
}