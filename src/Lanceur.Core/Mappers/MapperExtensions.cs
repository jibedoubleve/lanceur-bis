using Lanceur.Core.Models;

namespace Lanceur.Core.Mappers;

public static class MapperExtensions
{
    #region Methods

    public static void Rehydrate(this AliasQueryResult dest, AliasQueryResult src)
        => new MappingService().Rehydrate(src, dest);

    public static AliasQueryResult ToAliasQueryResult(this ExecutableQueryResult src)
        => new MappingService().ToAliasQueryResult(src);

    public static AliasQueryResult ToAliasQueryResult(this AliasUsageItem src)
        => new MappingService().ToAliasQueryResult(src);

    public static CompositeAliasQueryResult ToAliasQueryResultComposite(
        this AliasQueryResult src,
        IEnumerable<AliasQueryResult> aliases
    )
        => new MappingService().ToCompositeAliasQueryResult(src, aliases);

    public static IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(
        this IEnumerable<AliasQueryResult> src
    )
        => new MappingService().ToSelectableQueryResult(src);

    #endregion
}