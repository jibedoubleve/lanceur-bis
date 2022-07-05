﻿using Lanceur.Core.Models;

namespace Lanceur.Core.Services
{
    public interface IConvertionService
    {
        #region Methods

        CompositeAliasQueryResult ToAliasQueryResultComposite(AliasQueryResult source, IEnumerable<AliasQueryResult> subaliases);

        IEnumerable<QueryResult> ToQueryResult(IEnumerable<string> enumerable);

        IEnumerable<SelectableAliasQueryResult> ToSelectableQueryResult(IEnumerable<QueryResult> source);

        IEnumerable<SessionExecutableQueryResult> ToSessionExecutableQueryResult(IEnumerable<Session> source);

        #endregion Methods
    }
}