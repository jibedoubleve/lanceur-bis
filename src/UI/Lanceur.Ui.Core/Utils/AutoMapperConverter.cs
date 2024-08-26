using AutoMapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Utils;

public class AutoMapperConverter(IMapper mapper) : IConversionService
{
    #region Methods

    public CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> aliases)
    {
        var destination = new CompositeAliasQueryResult(aliases);
        var result = mapper.Map(source, destination);
        return result;
    }

    public IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> source) => mapper.Map<IEnumerable<string>, IEnumerable<DisplayQueryResult>>(source);

    public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<QueryResult> source) => mapper.Map<IEnumerable<QueryResult>, IEnumerable<SelectableAliasQueryResult>>(source);

    #endregion Methods
}