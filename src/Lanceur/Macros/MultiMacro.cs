using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Macros
{
    [Macro("multi"), Description("Allow to start multiple alias at once")]
    public class MultiMacro : MacroQueryResult
    {
        #region Fields

        private static readonly Conditional<int> DefaultDelay = new(0, 1_000);
        private readonly int _delay;
        private readonly IExecutionManager _executionManager;
        private readonly ISearchService _searchService;

        #endregion Fields

        #region Constructors

        public MultiMacro() : this(null)
        {
        }

        public MultiMacro(int? delay = null, IExecutionManager executionManager = null, ISearchService searchService = null)
        {
            _delay = delay ?? DefaultDelay;
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

        public override SelfExecutableQueryResult Clone() => this.CloneObject();

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var items = Parameters?.Split('@') ?? Array.Empty<string>();
            var aliases = items.Select(GetAlias).ToList();

            _ = _executionManager.ExecuteMultiple(aliases, _delay);

            return await NoResultAsync;
        }

        #endregion Methods
    }
}