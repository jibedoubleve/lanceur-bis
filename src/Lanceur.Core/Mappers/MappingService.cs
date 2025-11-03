using Lanceur.Core.Models;
using Riok.Mapperly.Abstractions;

namespace Lanceur.Core.Mappers;

[Mapper]
public partial class MappingService
{
    #region Methods

    public partial void Rehydrate(AliasQueryResult src, AliasQueryResult dst);

    /// <inheritdoc />
    [MapperRequiredMapping(RequiredMappingStrategy.None)]
    public partial AliasQueryResult ToAliasQueryResult(AliasUsageItem source);


    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial AliasQueryResult ToAliasQueryResult(ExecutableQueryResult src);

    public partial CompositeAliasQueryResult ToCompositeAliasQueryResult(
        AliasQueryResult source,
        IEnumerable<AliasQueryResult> aliases
    );

    [MapperIgnoreTarget(nameof(SelectableAliasQueryResult.IsSelected))]
    public partial SelectableAliasQueryResult ToSelectableAliasQueryResult(AliasQueryResult source);

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source)
        => source.Select(ToSelectableAliasQueryResult);

    #endregion
}