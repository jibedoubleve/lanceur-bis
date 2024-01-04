using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions
{
    internal class MacroDbAction
    {
        #region Fields

        private readonly IConvertionService _converter;
        private readonly IDbConnectionManager _db;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        #endregion Fields

        #region Constructors

        public MacroDbAction(IDbConnectionManager db, ILoggerFactory loggerFactory, IConvertionService converter)
        {
            _db = db;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MacroDbAction>();
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
            _logger.BeginSingleScope("AliasToHydrate", item);
            if (!item.IsComposite()) { return item; }

            var action = new AliasDbAction(_db, _loggerFactory);
            var subAliases = new List<AliasQueryResult>();

            var delay = 0;
            foreach (var name in item?.Parameters?.Split("@") ?? Array.Empty<string>())
            {
                if (name == string.Empty) { delay++; }
                else
                {
                    var alias = action.GetExact(name);
                    if (alias is null)
                    {
                        _logger.LogWarning("Impossible to create composite alias {AliasName}. Check all the items of the composite exists in the database", name);
                        continue;
                    }
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
            using var _ = _logger.MeasureExecutionTime(this);
            return collection.Select(Hydrate).ToList();
        }

        #endregion Methods
    }
}