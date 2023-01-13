using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Core.Utils
{
    public static class ExecutionMixin
    {
        #region Methods

        public static void Inject(this IEnumerable<QueryResult> queries, IExecutionManager service, Action<AliasQueryResult> onExecution)
        {
            foreach (var query in queries)
            {
                if (query is AliasQueryResult alias)
                {
                    alias.ExecutionManager = service;
                    alias.OnExecution = onExecution;
                }
            }
        }

        public static void Inject(this QueryResult query, IExecutionManager service, Action<AliasQueryResult> onExecution)
        {
            if (query is AliasQueryResult alias)
            {
                alias.ExecutionManager = service;
                alias.OnExecution = onExecution;
            }
        }

        #endregion Methods
    }
}