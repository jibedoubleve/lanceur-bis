using System.Reflection;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Stores;

public abstract class StoreBase
{
    #region Constructors

    protected StoreBase(
        IStoreOrchestrationFactory orchestrationFactory,
        ISection<StoreSection> storeSettings)
    {
        ArgumentNullException.ThrowIfNull(orchestrationFactory);
        ArgumentNullException.ThrowIfNull(storeSettings);

        StoreSettings = storeSettings;
        StoreOrchestrationFactory = orchestrationFactory;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Returns the effective shortcut for this store. If a user-defined override is configured,
    ///     it takes precedence over the default shortcut defined via <see cref="StoreAttribute" />.
    ///     Returns an empty string if neither is defined.
    /// </summary>
    protected string Shortcut
    {
        get
        {
            if (IsOverridable)
            {
                var overridenShortcut = StoreSettings.Value.GetOverride(this);

                if (!string.IsNullOrEmpty(overridenShortcut)) { return overridenShortcut; }
            }

            var attribute = GetType().GetCustomAttribute<StoreAttribute>();
            return attribute?.DefaultShortcut ?? string.Empty;
        }
    }

    protected IStoreOrchestrationFactory StoreOrchestrationFactory { get; }

    protected ISection<StoreSection> StoreSettings { get; }

    /// <summary>
    ///     Gets a value indicating whether the store's shortcut can be overridden
    ///     by the user through the configuration settings.
    /// </summary>
    public abstract bool IsOverridable { get; }

    #endregion

    #region Methods

    private static string SelectProperty(Cmdline cmdline) => cmdline.Name;

    /// <summary>
    ///     Determines whether <paramref name="current" /> is a valid refinement of
    ///     <paramref name="previous" />, according to a store-specific containment check.
    /// </summary>
    /// <remarks>
    ///     Use this helper in <see cref="CanPruneResult" /> to avoid duplicating the
    ///     refinement-check logic across stores. Pass a <paramref name="prunePolicy" />
    ///     that extracts the field the store actually searches on (e.g. <c>q => q.Name</c>
    ///     or <c>q => q.Parameters</c>), and a <paramref name="isRefinementOf" /> that matches
    ///     the store's own search semantics (e.g. <c>StartsWith</c> or <c>Contains</c>).
    ///     An empty previous search key is allowed and means "return all" — any refinement
    ///     of it is always a valid subset.
    /// </remarks>
    /// <param name="previous">The query used to produce the current result set. Can be <c>null</c>.</param>
    /// <param name="current">The new query to evaluate.</param>
    /// <param name="prunePolicy">Extracts the search key from a <see cref="Cmdline" />.</param>
    /// <param name="isRefinementOf">
    ///     Returns <c>true</c> if the current search key is a valid refinement of the previous one.
    ///     Must be consistent with the store's search semantics.
    /// </param>
    /// <returns>
    ///     <c>true</c> if <paramref name="isRefinementOf" /> confirms that the results of
    ///     <paramref name="current" /> are guaranteed to be a subset of the results of
    ///     <paramref name="previous" />; <c>false</c> otherwise.
    /// </returns>
    protected static bool OverrideCanPruneResult(
        Cmdline previous,
        Cmdline current,
        Func<Cmdline, string> prunePolicy,
        Func<string, string, bool> isRefinementOf)
    {
        var curr = prunePolicy(current);
        var prev = prunePolicy(previous);

        return isRefinementOf(prev, curr);
    }


    /// <summary>
    ///     Removes from <paramref name="destination" /> any result that no longer satisfies
    ///     <paramref name="current" />, using <paramref name="propSelector" /> to extract the
    ///     search key from the query and a custom <paramref name="isRefinementOf" /> to decide
    ///     which items to drop.
    /// </summary>
    /// <remarks>
    ///     Use this helper in a store's <c>PruneResult</c> override to avoid duplicating
    ///     removal logic. Pass a predicate that extracts the field the store searches on
    ///     (e.g. <c>q => q.Name</c> or <c>q => q.Parameters</c>). When <paramref name="isRefinementOf" />
    ///     is <c>null</c>, the default behaviour removes items whose <see cref="QueryResult.Name" />
    ///     does not start with the extracted key (case-insensitive).
    /// </remarks>
    /// <param name="destination">The current result set, modified in place.</param>
    /// <param name="previous">
    ///     The query used to produce the current result set. When <c>null</c>, no items are removed and
    ///     <c>0</c> is returned.
    /// </param>
    /// <param name="current">The new query to filter against.</param>
    /// <param name="propSelector">Extracts the search key from a <see cref="Cmdline" />.</param>
    /// <param name="isRefinementOf">
    ///     Optional. Returns <c>true</c> for items that should be removed. When <c>null</c>,
    ///     items whose <see cref="QueryResult.Name" /> does not start with
    ///     <c>predicate(<paramref name="current" />)</c> are removed.
    /// </param>
    /// <returns>The number of items removed from <paramref name="destination" />.</returns>
    protected static int OverridePruneResult(
        IList<QueryResult> destination,
        Cmdline previous,
        Cmdline current,
        Func<Cmdline, string> propSelector,
        Func<QueryResult, string, bool> isRefinementOf)
    {
        var currValue = propSelector(current);
        var prevValue = propSelector(previous);

        var toDelete = destination.Where(i => isRefinementOf(i, prevValue)
                                              && !isRefinementOf(i, currValue))
                                  .ToArray();

        destination.RemoveMultiple(toDelete);
        return toDelete.Length;
    }

    /// <summary>
    ///     Determines whether the current result set can be refined by pruning
    ///     instead of executing a full search again.
    /// </summary>
    /// <remarks>
    ///     Returns <c>true</c> when the new query is a logical refinement of the
    ///     previous one and the store's search semantics guarantee that the results
    ///     of <paramref name="current" /> are a subset of the results of
    ///     <paramref name="previous" />. In that case, <see cref="PruneResult" /> can
    ///     be called instead of <see cref="Lanceur.Core.Services.IStoreService.Search" />.
    /// </remarks>
    /// <param name="previous">The query used to produce the current result set.</param>
    /// <param name="current">The new query to evaluate.</param>
    /// <returns>
    ///     <c>true</c> if the existing results can be pruned to satisfy
    ///     <paramref name="current" />; <c>false</c> if a full search is required.
    /// </returns>
    public virtual bool CanPruneResult(Cmdline previous, Cmdline current)
        => OverrideCanPruneResult(
            previous,
            current,
            SelectProperty,
            (prev, cur) => cur.StartsWith(prev, StringComparison.InvariantCultureIgnoreCase)
        );

    public virtual IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <summary>
    ///     Removes from <paramref name="destination" /> any result that no longer
    ///     satisfies <paramref name="current" />, according to this store's search
    ///     semantics.
    /// </summary>
    /// <remarks>
    ///     This method is only called when <see cref="CanPruneResult" /> returned
    ///     <c>true</c>. Each store is responsible for applying its own filtering
    ///     logic (e.g. prefix match on <see cref="Cmdline.Name" />, substring match
    ///     on <see cref="Cmdline.Parameters" />). The list is modified in place.
    /// </remarks>
    /// <param name="destination">The current result set, modified in place.</param>
    /// <param name="previous">
    ///     The query used to produce the current result set. When <c>null</c>, no items are removed and
    ///     <c>0</c> is returned.
    /// </param>
    /// <param name="current">The new query to filter against.</param>
    /// <returns>The number of items removed from <paramref name="destination" />.</returns>
    public virtual int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current)
        => OverridePruneResult(
            destination,
            previous,
            current,
            SelectProperty,
            (item, value) => item.Name.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
        );

    #endregion
}