using Lanceur.Core.Models;
using Riok.Mapperly.Abstractions;

namespace Lanceur.Core.Mappers;

[Mapper]
public partial class AliasQueryResultMapper
{
    #region Methods

    public partial void Rehydrate(AliasQueryResult src, AliasQueryResult dst);
    
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial AliasQueryResult Map(ExecutableQueryResult src);
    #endregion
}