using System.Reflection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Core.Configuration;

public class ConfigurationSection<T> : IWriteableSection<T>
    where T : class
{
    #region Fields

    private T _cachedSection  ;

    private readonly IConfigurationFacade _configuration;

    #endregion

    #region Constructors

    public ConfigurationSection(IConfigurationFacade configuration)
    {
        _configuration = configuration;
        _configuration.Updated += (_, _) => _cachedSection = RebuildSection();
    }

    #endregion

    #region Properties

    public T Value
    {
        get
        {
            _cachedSection ??= RebuildSection();
            return _cachedSection;
        }
    }

    #endregion

    #region Methods

    private T RebuildSection()
    {
        var app = _configuration.Application;
        return app.GetType()
                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
                  .Select(p => (T)p.GetValue(app))
                  .SingleOrDefault();
    }

    public void Reload() => _configuration.Reload();

    public void Save() => _configuration.Save();

    #endregion
}