using System.Reflection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Configuration;

public class ConfigurationSection<T> : IWriteableSection<T>
    where T : class
{
    #region Fields

    private T _cachedSection  ;

    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public ConfigurationSection(ISettingsFacade settings, ILogger<ConfigurationSection<T>> logger)
    {
        _settings = settings;
        _settings.Updated += (_, _) => _cachedSection = RebuildSection();
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