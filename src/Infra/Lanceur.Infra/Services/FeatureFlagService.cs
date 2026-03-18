using Lanceur.Core.Repositories;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public sealed class FeatureFlagService : IFeatureFlagService
{
    #region Fields

    private readonly IFeatureFlagRepository _repository;

    #endregion

    #region Constructors

    public FeatureFlagService(IFeatureFlagRepository repository) => _repository = repository;

    #endregion

    #region Methods

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
        => _repository.GetFeatureFlags()
                      .Where(e => string.Equals(e.FeatureName, featureName, StringComparison.CurrentCultureIgnoreCase))
                      .Any(e => e.Enabled);

    #endregion
}