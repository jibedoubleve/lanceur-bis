using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;

namespace Lanceur.Tests.Tools.Stubs;

public class TestStore : StoreBase, IStoreService
{
    #region Fields

    private readonly Func<Cmdline, string> _prunePolicy;

    private IEnumerable<QueryResult> _results = [];

    #endregion

    #region Constructors

    public TestStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ISection<StoreSection> storeSettings,
        Func<Cmdline, string> prunePolicy) : base(orchestrationFactory, storeSettings)
        => _prunePolicy = prunePolicy;

    #endregion

    #region Properties

    /// <inheritdoc cref="IStoreService.IsOverridable" />
    public override bool IsOverridable => true;

    public int SearchCallCount { get; private set; }
    public StoreOrchestration Orchestration => StoreOrchestrationFactory.SharedAlwaysActive();

    #endregion

    #region Methods

    private static bool IsRefinementOf(string value, string item)
        => item.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);

    public override bool CanPruneResult(Cmdline previous, Cmdline current)
        => OverrideCanPruneResult(
            previous,
            current,
            _prunePolicy,
            IsRefinementOf
        );

    public override int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current)
        => OverridePruneResult(
            destination,
            previous,
            current,
            _prunePolicy,
            (item, value) => IsRefinementOf(value, item.Name)
        );

    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        SearchCallCount++;
        return _results.Where(item => IsRefinementOf(_prunePolicy(cmdline), item.Name));
    }

    /// <summary>
    ///     Seeds the result set that <see cref="Search" /> will filter against.
    /// </summary>
    /// <remarks>
    ///     Call this before triggering a search. <see cref="Search" /> still applies the
    ///     store's <c>prunePolicy</c> to this set, so only items whose name satisfies the
    ///     policy for the given query are returned.
    /// </remarks>
    /// <param name="results">The candidate results to filter on each <see cref="Search" /> call.</param>
    public void SeedResultSet(IEnumerable<QueryResult> results) => _results = results;

    #endregion
}