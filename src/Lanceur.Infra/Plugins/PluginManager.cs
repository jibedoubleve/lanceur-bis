using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Utils;
using Splat;
using System.Reflection;

namespace Lanceur.Infra.Plugins
{
    public class PluginManager : IPluginManager
    {
        #region Fields

        private readonly IAppLogger _log;
        private readonly IPluginStoreContext _pluginStoreContext;

        #endregion Fields

        #region Constructors

        public PluginManager(IPluginStoreContext pluginStoreContext = null, IAppLoggerFactory logFactory = null)
        {
            var l = Locator.Current;
            _pluginStoreContext = pluginStoreContext ?? l.GetService<IPluginStoreContext>();
            _log = l.GetLogger<PluginManager>(logFactory);
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<IPlugin> CreatePlugin(Assembly assembly)
        {
            int count = 0;
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is IPlugin result)
                    {
                        count++;
                        _log.Info($"Found plugin '{result.Name}'");
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                _log.Warning(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }

        public Assembly LoadPluginAsm(string path)
        {
            var root = _pluginStoreContext.RepositoryPath;
            var pluginLocation = Path.GetFullPath(Path.Combine(root, path.Replace('\\', Path.DirectorySeparatorChar)));
            var ctx = new PluginLoadContext(pluginLocation);

            var filename = Path.GetFileNameWithoutExtension(pluginLocation);
            return ctx.LoadFromAssemblyName(new AssemblyName(filename));
        }

        #endregion Methods
    }
}