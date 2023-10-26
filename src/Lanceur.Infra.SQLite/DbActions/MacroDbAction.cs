using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.SQLite.DbActions
{
    internal class MacroDbAction
    {
        #region Fields

        private readonly IConvertionService _converter;
        private readonly ISQLiteConnectionScope _db;
        private readonly IAppLoggerFactory _log;

        #endregion Fields

        #region Constructors

        public MacroDbAction(ISQLiteConnectionScope db, IAppLoggerFactory log, IConvertionService converter)
        {
            _db = db;
            _log = log;
            _converter = converter;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// If the specified <see cref="AliasQueryResult"/> is composite
        /// this method will hydrate and create a <see cref="CompositeAliasQueryResult"/>;
        /// otherwise it'll return <paramref name="item"/>
        /// </summary>
        /// <param name="item">The <see cref="QueryResult"/> to hydrate</param>
        /// <returns>The hydrated <see cref="QueryResult"/> or <paramref name="item"/></returns>
        private AliasQueryResult Hydrate(AliasQueryResult item)
        {
            if (!item.IsComposite()) { return item; }

            var action = new AliasDbAction(_db, _log);
            var subAliases = new List<AliasQueryResult>();

            int delay = 0;
            foreach (var name in item?.Parameters?.Split("@") ?? Array.Empty<string>())
            {
                if (name == string.Empty) { delay++; }
                else
                {
                    var alias = action.GetExact(name);
                    alias.Delay = delay;

                    subAliases.Add(alias);
                    delay = 1;
                }
            }

            return _converter.ToAliasQueryResultComposite(item, subAliases);
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
                items.Add(Hydrate(item));
            }
            return items;
        }

        #endregion Methods
    }
}