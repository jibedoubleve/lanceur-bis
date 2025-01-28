using System.Data;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class MacroDbAction
{
    #region Fields

    private readonly IMappingService _converter;
    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    #endregion

    #region Constructors

    internal MacroDbAction(ILoggerFactory loggerFactory, IMappingService converter, IDbActionFactory dbActionFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MacroDbAction>();
        _converter = converter;
        _dbActionFactory = dbActionFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     If the specified <see cref="AliasQueryResult" /> is composite
    ///     this method will hydrate and create a <see cref="CompositeAliasQueryResult" />;
    ///     otherwise it'll return <paramref name="item" />
    /// </summary>
    /// <param name="tx">The transaction to use for this query</param>
    /// <param name="item">The <see cref="QueryResult" /> to hydrate</param>
    /// <returns>The hydrated <see cref="QueryResult" /> or <paramref name="item" /></returns>
    private AliasQueryResult Hydrate(IDbTransaction tx, AliasQueryResult item)
    {
        _logger.BeginSingleScope("AliasToHydrate", item);
        if (!item.IsComposite()) return item;

        var action = _dbActionFactory.AliasManagement;
        var subAliases = new List<AliasQueryResult>();

        var names = item?.Parameters?.Split("@") ?? [];
        var aliases = action.GetExact(tx, names).ToArray();

        var delay = 0;
        foreach (var name in names)
            if (name == string.Empty) { delay++; }
            else
            {
                var alias = aliases.FirstOrDefault(a => a.Name == name);
                if (alias is null)
                {
                    _logger.LogWarning("Impossible to create composite alias {AliasName}. Check all the items of the composite exists in the database", name);
                    continue;
                }

                alias.Delay = delay;

                subAliases.Add(alias);
                delay = 1;
            }

        var result = _converter.ToAliasQueryResultComposite(item, subAliases);
        return result;
    }

    /// <summary>
    ///     Go through the collection and upgrade items to composite when they are
    ///     composite macro.
    /// </summary>
    /// <param name="tx">The transaction to use for this query</param>
    /// <param name="collection">The collection to upgrade</param>
    /// <returns>
    ///     The collection with all element that are upgradable
    ///     to composite, upgraded
    /// </returns>
    internal IEnumerable<AliasQueryResult> UpgradeToComposite(IDbTransaction tx, IEnumerable<AliasQueryResult> collection)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        var list = new List<AliasQueryResult>(collection);
        var composites = list.Where(item => false == item.FileName.IsNullOrEmpty())
                             .Where(item => item.FileName.ToUpper().Contains("@MULTI@"))
                             .Select(x => Hydrate(tx, x))
                             .ToArray();

        list.RemoveAll(x => composites.Select(c => c.Id).Contains(x.Id));
        list.AddRange(composites);

        return list.ToArray();
    }

    #endregion
}