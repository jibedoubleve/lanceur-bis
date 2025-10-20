using System.Reflection;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Core.Configuration;

public class ConfigurationSection<T> : IWriteableSection<T>
    where T : class
{
    #region Fields

    private T _cachedSection  ;

    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public ConfigurationSection(ISettingsFacade settings) => _settings = settings;

    #endregion

    #region Properties

    public T Value
    {
        get
        {
            _cachedSection ??= BuildSection();
            return _cachedSection;
        }
    }

    #endregion

    #region Methods

    private T BuildSection()
    {
        var app = _settings.Application;
        return app.GetType()
                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
                  .Select(p => (T)p.GetValue(app))
                  .SingleOrDefault();
    }

    public void Reload() => _settings.Reload();

    public void Save() => _settings.Save();

    #endregion
}