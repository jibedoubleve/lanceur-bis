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
        StoreSettings = storeSettings;
        StoreOrchestrationFactory
            = orchestrationFactory ??
              throw new ArgumentException(
                  $"The {typeof(IStoreOrchestrationFactory)} should be configured in the IOC container."
              );
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Returns the default shortcut defined for the macro. If no shortcut defined, then empty string
    ///     is returned
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