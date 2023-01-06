using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
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
    public class MultiMacro : ExecutableQueryResult
    {
        #region Fields

        private readonly int _delay;

        private readonly IExecutionManager _executionManager;
        private readonly ISearchService _searchService;

        #endregion Fields

        #region Constructors
#if DEBUG
        public MultiMacro() : this(0, null, null) { }
#else

        public MultiMacro() : this(null, null, null) { }
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
            var items = cmdline?.Parameters?.Split('@') ?? Array.Empty<string>();

            foreach (var item in items)
            {
                await Task.Delay(_delay);
                var alias = GetAlias(item);
                if (alias is not null) { await _executionManager.ExecuteAsync(alias); }
            }

            return NoResult;
        }

        #endregion Methods
    }
}