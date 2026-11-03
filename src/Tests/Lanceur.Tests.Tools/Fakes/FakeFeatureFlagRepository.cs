using Lanceur.Core.Models;
using Lanceur.Core.Repositories;

namespace Lanceur.Tests.Tools.Fakes;

public class FakeFeatureFlagRepository : IFeatureFlagRepository
{
    #region Methods

    public IEnumerable<FeatureFlag> GetFeatureFlags() => [];

    #endregion
}