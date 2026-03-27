using System.Reflection;
using Lanceur.Core.Services;

namespace Lanceur.Core.Configuration;

/// <summary>
///     Resolves a configuration section of type <typeparamref name="T" /> from a set of <see cref="ISettingsProvider" />.
/// </summary>
/// <typeparam name="T">The type of the configuration section to resolve. Must be a reference type.</typeparam>
/// <remarks>
///     <para>
///         At most one provider may expose a property of type <typeparamref name="T" />, and each provider
///         may expose it at most once. Duplicate registrations across providers are considered a
///         programming error and will throw at resolution time.
///     </para>
///     <para>
///         <see cref="Value" /> is guaranteed non-null; if no provider exposes <typeparamref name="T" />,
///         an <see cref="InvalidOperationException" /> is thrown.
///     </para>
/// </remarks>
public sealed class Section<T> : IWriteableSection<T>
    where T : class
{
    #region Fields

    private readonly IEnumerable<ISettingsProvider> _settingsProviders;

    private T? _value;

    #endregion

    #region Constructors

    public Section(IEnumerable<ISettingsProvider> settingsProviders) => _settingsProviders = settingsProviders;

    #endregion

    #region Properties

    /// <inheritdoc />
    public T Value
        => RebuildSections()
           ?? throw new InvalidOperationException(
               $"No configuration section of type '{typeof(T).Name}' was found. " +
               "Ensure it is registered in a settings provider.");

    #endregion

    #region Methods

    private static T? RebuildCurrentSection(ISettingsProvider src)
    {
        // get all the properties of the section (not static and public) 
        var properties = src.Value.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Select the property that matches the type T
        var result = properties.Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
                               .Select(p => (T?)p.GetValue(src.Value))
                               .SingleOrDefault();
        return result;
    }

    /// <summary>
    ///     Rebuilds the section of type <typeparamref name="T" /> from all registered providers.
    /// </summary>
    /// <remarks>
    ///     Each type <typeparamref name="T" /> must appear in at most one provider and at most once
    ///     per provider. Duplicate registrations indicate a misconfiguration and will throw.
    /// </remarks>
    private T? RebuildSections()
        => _settingsProviders.Select(RebuildCurrentSection)
                             .OfType<T>()
                             .SingleOrDefault(); // Intentional: ambiguous config is a programming error

    /// <inheritdoc />
    public void Reload()
    {
        foreach (var provider in _settingsProviders)
        {
            provider.Load();
        }

        _value = null; // Invalidate cache so Value re-resolves on next access
    }

    /// <inheritdoc />
    public void Save()
    {
        foreach (var provider in _settingsProviders)
        {
            provider.Save();
        }
    }

    #endregion
}