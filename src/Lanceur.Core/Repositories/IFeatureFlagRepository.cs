using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories;

public interface IFeatureFlagRepository
{
    #region Methods

    /// <summary>
    ///     Retrieves a list of all feature flags along with their states.
    /// </summary>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="FeatureFlag" /> representing all feature flags.
    /// </returns>
    IEnumerable<FeatureFlag> GetFeatureFlags();

    #endregion
}