using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Service for managing feature flags.
/// </summary>
public interface IFeatureFlagService
{
    #region Methods

    /// <summary>
    ///     Checks whether a specified feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <returns>
    ///     <c>true</c> if the feature flag is enabled; otherwise, <c>false</c>.
    /// </returns>
    bool IsEnabled(string featureName);

    #endregion
}