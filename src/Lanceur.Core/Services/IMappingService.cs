using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IMappingService
{
    #region Methods

    CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> subaliases);

    IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> enumerable);

    IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source);

    #endregion Methods
}