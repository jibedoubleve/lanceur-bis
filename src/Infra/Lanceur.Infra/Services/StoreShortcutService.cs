using System.Reflection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public class StoreShortcutService : IStoreShortcutService
{
    #region Fields

    private readonly IEnumerable<IStoreService> _stores;

    #endregion

    #region Constructors

    public StoreShortcutService(IEnumerable<IStoreService> stores) => _stores = stores;

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<StoreShortcut> Resolve(StoreSection storeSection)
    {
        var discovered = _stores
            .Where(s => s.IsOverridable)
            .Select(s => new StoreShortcut
            {
                StoreType    = s.GetType().FullName!,
                AliasOverride = s.GetType().GetCustomAttribute<StoreAttribute>()?.DefaultShortcut
            });

        var existingTypes = storeSection.StoreShortcuts.Select(s => s.StoreType).ToHashSet();
        return storeSection.StoreShortcuts.Concat(discovered.Where(x => !existingTypes.Contains(x.StoreType)));
    }

    #endregion
}