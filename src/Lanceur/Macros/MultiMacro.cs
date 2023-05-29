using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lanceur.Macros
{
    [Macro("multi"), Description("Allow to start multiple alias at once")]
    public class MultiMacro : SelfExecutableQueryResult
    {
        #region Fields

        private readonly int _delay;

        private readonly IExecutionManager _executionManager;
        private readonly ISearchService _searchService;

        #endregion Fields

        #region Constructors

#if DEBUG

        public MultiMacro() : this(0, null, null)
        {
        }

#else

        public MultiMacro() : this(null, null, null)
        {
        }

#endif

        public MultiMacro(int? delay = null, IExecutionManager executionManager = null, ISearchService searchService = null)
        {
            _delay = delay ?? 1_000;
            _executionManager = executionManager ?? Locator.Current.GetService<IExecutionManager>();
            _searchService = searchService ?? Locator.Current.GetService<ISearchService>();
        }

        #endregion Constructors

        #region Methods

        private AliasQueryResult GetAlias(string item)
        {
            var cmdline = new Cmdline(item);
            var macro = _searchService
                .Search(cmdline)
                .FirstOrDefault();
            return macro as AliasQueryResult;
        }

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var items = Parameters?.Split('@') ?? Array.Empty<string>();

            foreach (var item in items)
            {
                await Task.Delay(_delay);
                var alias = GetAlias(item);
                if (alias is not null)
                {
#pragma warning disable 4014
                    //https://stackoverflow.com/a/20364016/389529
                    _executionManager.ExecuteAsync(new ExecutionRequest
                    {
                        QueryResult = alias,
                    }).ConfigureAwait(false);
#pragma warning disable 4014
                }
            }

            return await NoResultAsync;
        }

        #endregion Methods
    }
}