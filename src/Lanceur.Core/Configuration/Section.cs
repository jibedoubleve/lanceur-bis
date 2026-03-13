using System.Reflection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Services;

namespace Lanceur.Core.Configuration;

public class Section<T> : IWriteableSection<T>
    where T : class
{
    #region Fields

    private readonly IEnumerable<ISettingsProvider> _settingsProviders;

    #endregion

    #region Constructors

    public Section(IEnumerable<ISettingsProvider> settingsProviders) => _settingsProviders = settingsProviders;

    #endregion

    #region Properties

    public T Value
    {
        get
        {
            field ??= RebuildSections();
            return field!;
        }
    }

    #endregion

    #region Methods

    private static T? RebuildSectionGroup(ISettingsProvider src)
    {
        var properties = src.Current.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        var result = properties.Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
                               .Select(p => (T?)p.GetValue(src.Current))
                               .SingleOrDefault();
        return result;
    }

    private T? RebuildSections()
        => _settingsProviders.Select(RebuildSectionGroup)
                             .OfType<T>()
                             .FirstOrDefault();

    public void Reload()
    {
        foreach (var provider in _settingsProviders)
        {
            provider.Load();
        }
    }

    public void Save()
    {
        foreach (var provider in _settingsProviders)
        {
            provider.Save();
        }
    }

    #endregion
}