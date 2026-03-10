using System.Reflection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Core.Configuration;

public class ConfigurationSection<T> : ISection<T>, IWriteableSection<T>
    where T : class
{
    #region Fields

    private T? _cachedSection;

    private readonly IConfigurationFacade _configuration;

    #endregion

    #region Constructors

    public ConfigurationSection(IConfigurationFacade configuration)
    {
        _configuration = configuration;
        _configuration.Updated += (_, _) => _cachedSection = RebuildSections();
    }

    #endregion

    #region Properties

    public T Value
    {
        get
        {
            _cachedSection ??= RebuildSections();
            return _cachedSection!;
        }
    }

    #endregion

    #region Methods

    private static T? RebuildSectionGroup(object src)
        => src.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
              .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
              .Select(p => (T?)p.GetValue(src))
              .SingleOrDefault();

    private T? RebuildSections()
        => RebuildSectionGroup(_configuration.Application)
           ?? RebuildSectionGroup(_configuration.Local);

    public void Reload() => _configuration.Reload();

    public void Save() => _configuration.Save();

    #endregion
}