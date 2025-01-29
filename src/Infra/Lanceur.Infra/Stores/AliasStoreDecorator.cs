using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Stores;

/// <summary>
///     This store acts as a decorator for the <see cref="AliasStore" /> and filters out
///     all invisible aliases from the results. Specifically, it removes any
///     <see cref="AliasQueryResult" /> where the <c>IsHidden</c> property is set to <c>true</c>.
/// </summary>
public class AliasStoreDecorator : IStoreService
{
    #region Fields

    private readonly AliasStore _aliasStore;

    #endregion

    #region Constructors

    public AliasStoreDecorator(AliasStore aliasStore) => _aliasStore = aliasStore;

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => _aliasStore.IsOverridable;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => _aliasStore.StoreOrchestration;

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _aliasStore.GetAll()
                                                           .Cast<AliasQueryResult>()
                                                           .Where(x => x.IsHidden == false)
                                                           .OrderBy(x => x.Name);

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query) => _aliasStore.Search(query)
                                                                        .Cast<AliasQueryResult>()
                                                                        .Where(x => x.IsHidden == false)
                                                                        .OrderBy(x => x.Name);

    #endregion
}