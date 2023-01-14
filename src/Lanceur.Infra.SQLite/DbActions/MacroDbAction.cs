using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.SQLite.DbActions
{
    internal class MacroDbAction
    {
        #region Fields

        private readonly IConvertionService _converter;
        private readonly SQLiteConnectionScope _db;
        private readonly IExecutionManager _executionService;
        private readonly ILogService _log;

        #endregion Fields

        #region Constructors

        public MacroDbAction(SQLiteConnectionScope db, ILogService log, IConvertionService converter)
        {
            _db = db;
            _log = log;
            _converter = converter;
        }

        #endregion Constructors

        #region Methods

        private AliasQueryResult GetComposite(AliasQueryResult item)
        {
            if (item.Is(CompositeMacros.Multi))
            {
                var action = new AliasDbAction(_db, _log);
                var subAliases = new List<AliasQueryResult>();

                int delay = 0;
                foreach (var name in item?.Arguments?.Split("@") ?? Array.Empty<string>())
                {
                    if (name == string.Empty) { delay++; }
                    else
                    {
                        subAliases.Add(item: action.GetExact(name, delay: delay));
                        delay = 1;
                    }
                }

                return _converter.ToAliasQueryResultComposite(item, subAliases);
            }
            else { return item; }
        }

        /// <summary>
        /// Go through the collection and upgrade items to composite when they are
        /// composite macro.
        /// </summary>
        /// <param name="collection">The collection to upgrade</param>
        /// <returns>
        /// The collection with all element that are upgradable 
        /// to composite, upgraded
        /// </returns>
        public IEnumerable<AliasQueryResult> UpgradeToComposite(IEnumerable<AliasQueryResult> collection)
        {
            var items = new List<AliasQueryResult>();

            foreach (var item in collection)
            {
                items.Add(item.IsComposite() ? GetComposite(item) : item);
            }
            return items;
        }

        #endregion Methods
    }
}