namespace Lanceur.Core.Models;

public sealed class CompositeAliasQueryResult : AliasQueryResult
{
    #region Constructors

    public CompositeAliasQueryResult(IEnumerable<AliasQueryResult> aliases) => Aliases = aliases;

    #endregion

    #region Properties

    public IEnumerable<AliasQueryResult> Aliases { get; }

    #endregion
}