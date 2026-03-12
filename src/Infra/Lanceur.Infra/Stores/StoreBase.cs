using System.Reflection;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Stores;

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
    protected string DefaultShortcut
    {
        get
        {
            var overridenShortcut = StoreSettings.Value.GetOverride(this);

            if (!string.IsNullOrEmpty(overridenShortcut)) { return overridenShortcut; }

            var attribute = GetType().GetCustomAttribute<StoreAttribute>();
            return attribute?.DefaultShortcut ?? string.Empty;
        }
    }

    protected IStoreOrchestrationFactory StoreOrchestrationFactory { get; }

    protected ISection<StoreSection> StoreSettings { get; }

    #endregion

    #region Methods

    public virtual IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    #endregion
}