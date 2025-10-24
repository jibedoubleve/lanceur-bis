using Lanceur.Core.Models;

namespace Lanceur.Core.Mappers;

public static class MapperExtensions
{
    #region Methods

    public static void Rehydrate(this AliasQueryResult dest, AliasQueryResult src)
        => new AliasQueryResultMapper().Rehydrate(src, dest);

    public static AliasQueryResult ToAliasQueryResult(this ExecutableQueryResult src)
        => new AliasQueryResultMapper().Map(src);

    #endregion
}