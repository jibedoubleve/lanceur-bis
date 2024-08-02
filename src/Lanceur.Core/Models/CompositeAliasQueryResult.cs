namespace Lanceur.Core.Models;

public class CompositeAliasQueryResult : AliasQueryResult
{
    #region Constructors

    public CompositeAliasQueryResult(IEnumerable<AliasQueryResult> aliases) => Aliases = aliases;

    #endregion Constructors

    #region Properties

    public IEnumerable<AliasQueryResult> Aliases { get; }

    #endregion Properties
}