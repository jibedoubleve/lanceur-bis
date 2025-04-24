using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Riok.Mapperly.Abstractions;

namespace Lanceur.Core.Mappers;

[Mapper]
public partial class MappingService : IMappingService
{
    #region Methods

    private partial CompositeAliasQueryResult ToCompositeAliasQueryResult(AliasQueryResult source, IEnumerable<AliasQueryResult> aliases);

    private static DisplayQueryResult ToQueryResult(string source) => new($"@{source}@", "This is a macro", "LinkVariant");

    [MapperIgnoreTarget(nameof(SelectableAliasQueryResult.IsSelected))]
    private partial SelectableAliasQueryResult ToSelectableAliasQueryResult(AliasQueryResult source);

    /// <inheritdoc />
    public CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> subaliases) => ToCompositeAliasQueryResult(source, subaliases);

    /// <inheritdoc />
    public  IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> enumerable)
    {
        enumerable = enumerable?.ToList() ?? [];
        return enumerable.Any()
            ? enumerable.Select(ToQueryResult)
            : [];
    }

    /// <inheritdoc />
    public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source) => source.Select(ToSelectableAliasQueryResult);

    #endregion
}