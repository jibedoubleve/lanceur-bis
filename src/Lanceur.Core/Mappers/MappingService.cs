using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Riok.Mapperly.Abstractions;

namespace Lanceur.Core.Mappers;

[Mapper]
public partial class MappingService
{
    #region Methods

    public partial CompositeAliasQueryResult ToCompositeAliasQueryResult(AliasQueryResult source, IEnumerable<AliasQueryResult> aliases);

    [MapperIgnoreTarget(nameof(SelectableAliasQueryResult.IsSelected))]
    public partial SelectableAliasQueryResult ToSelectableAliasQueryResult(AliasQueryResult source);

    /// <inheritdoc />
    [MapperRequiredMapping(RequiredMappingStrategy.None)]
    public partial AliasQueryResult ToAliasQueryResult(AliasUsageItem source);

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source) => source.Select(ToSelectableAliasQueryResult);

    #endregion
}