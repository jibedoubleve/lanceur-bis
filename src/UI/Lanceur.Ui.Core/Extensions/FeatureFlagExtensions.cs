using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Extensions;

public static class FeatureFlagExtensions
{
    #region Methods

    public static bool IsFeatureFlagEnabled(this IEnumerable<FeatureFlag> flags, string featureFlagName)
    {
        return flags.Any(e =>
            e.FeatureName.Equals(featureFlagName, StringComparison.OrdinalIgnoreCase) && e.Enabled
        );
    }

    #endregion
}