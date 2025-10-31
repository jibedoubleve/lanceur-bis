using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IMappingService
{
    #region Methods

    CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> subaliases);

    AliasQueryResult ToAliasQueryResult(AliasUsageItem source);

    IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source);

    #endregion Methods
}