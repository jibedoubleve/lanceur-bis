using Lanceur.Core.Models;
using Lanceur.Core.Repositories;

namespace Lanceur.Tests.Tools.Stubs;

public class TestFeatureFlagRepository : IFeatureFlagRepository
{
    #region Methods

    public IEnumerable<FeatureFlag> GetFeatureFlags() => [];

    #endregion
}