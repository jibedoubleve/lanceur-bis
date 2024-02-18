using AutoMapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using System.Collections.Generic;

namespace Lanceur.Utils
{
    public class AutoMapperConverter : IConversionService
    {
        #region Fields

        private readonly IMapper _mapper;

        #endregion Fields

        #region Constructors

        public AutoMapperConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        #endregion Constructors

        #region Methods

        public CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> aliases)
        {
            var destination = new CompositeAliasQueryResult(aliases);
            var result = _mapper.Map(source, destination);
            return result;
        }

        public IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> source)
        {
            return _mapper.Map<IEnumerable<string>, IEnumerable<DisplayQueryResult>>(source);
        }

        public IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<QueryResult> source)
        {
            return _mapper.Map<IEnumerable<QueryResult>, IEnumerable<SelectableAliasQueryResult>>(source);
        }

        public IEnumerable<SessionExecutableQueryResult> ToSessionExecutableQueryResult(IEnumerable<Session> source)
        {
            return _mapper.Map<IEnumerable<Session>, IEnumerable<SessionExecutableQueryResult>>(source);
        }

        #endregion Methods
    }
}