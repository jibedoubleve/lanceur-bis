using AutoMapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Utils;

public class AutoMapperMappingService : IMappingService
{
    #region Fields

    private static readonly IMapper Mapper;

    #endregion

    #region Constructors

    static AutoMapperMappingService()
    {
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<AliasQueryResult, SelectableAliasQueryResult>();
                cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();

                cfg.CreateMap<string, DisplayQueryResult>()
                   .ConstructUsing(x => new($"@{x}@", "This is a macro", "LinkVariant"));
            }
        );
        Mapper = new Mapper(configuration);
    }

    #endregion

    #region Methods

    public CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> aliases)
    {
        var destination = new CompositeAliasQueryResult(aliases);
        var result = Mapper.Map(source, destination);
        return result;
    }

    public IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> source) => Mapper.Map<IEnumerable<string>, IEnumerable<DisplayQueryResult>>(source);

    public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<AliasQueryResult> source) => Mapper.Map<IEnumerable<AliasQueryResult>, IEnumerable<SelectableAliasQueryResult>>(source);

    #endregion
}