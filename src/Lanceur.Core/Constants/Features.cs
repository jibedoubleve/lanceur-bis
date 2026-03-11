using System.Reflection;

namespace Lanceur.Core.Constants;

public static class Features
{
    #region Fields

    public const string AdditionalParameterAlwaysActive = "AdditionalParameterAlwaysActive";
    public const string ResourceDisplay = "ShowSystemUsage";
    public const string SteamIntegration = "SteamLibraryGames";

    #endregion

    #region Methods

    /// <summary>
    ///     Retrieves the constant values of all public string features defined in <see cref="Features" />.
    /// </summary>
    /// <remarks>
    ///     This method uses reflection to dynamically discover feature flags.
    ///     It filters for fields that are both <see langword="public" /> and <see langword="const" />.
    /// </remarks>
    /// <returns>An enumeration of strings representing the defined feature flag values.</returns>
    public static IEnumerable<string> GetNames() =>
        typeof(Features)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!);

    #endregion
}