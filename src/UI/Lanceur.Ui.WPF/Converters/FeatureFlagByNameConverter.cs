using System.Globalization;
using System.Windows.Data;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Converters;

/// <summary>
///     Selects, from a collection of <see cref="FeatureFlag" />, the flag whose
///     <see cref="FeatureFlag.FeatureName" /> matches the converter parameter.
/// </summary>
/// <remarks>
///     Lets a view bind a single toggle to a given feature flag, next to the settings of that feature.
/// </remarks>
public sealed class FeatureFlagByNameConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<FeatureFlag> flags || parameter is not string name) { return null; }

        return flags.FirstOrDefault(f => f.FeatureName == name);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    #endregion
}
