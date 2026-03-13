using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Provides the complete list of store shortcuts, merging discovered shortcuts
///     with configuration overrides.
/// </summary>
public interface IStoreShortcutService
{
    #region Methods

    /// <summary>
    ///     Resolves the effective list of store shortcuts by combining all shortcuts
    ///     discovered via reflection with those defined in <paramref name="storeSection" />.
    ///     Shortcuts already present in the configuration take precedence (overrides),
    ///     and any discovered shortcut not yet in the configuration is appended.
    /// </summary>
    /// <param name="storeSection">
    ///     The configuration section containing user-defined shortcut overrides.
    /// </param>
    /// <returns>
    ///     The complete set of effective store shortcuts, with configuration overrides applied.
    /// </returns>
    IEnumerable<StoreShortcut> Resolve(StoreSection storeSection);

    #endregion
}