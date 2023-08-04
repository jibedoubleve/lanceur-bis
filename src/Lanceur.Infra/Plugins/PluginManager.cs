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

        private Assembly LoadPluginAsm(string relativePath)
        {
            var loc = PluginLocation.FromFile(relativePath).FullPath;

            var ctx = new PluginLoadContext(loc);

            var filename = Path.GetFileNameWithoutExtension(loc);
            return ctx.LoadFromAssemblyName(new AssemblyName(filename));
        }

        public IEnumerable<IPlugin> CreatePlugin(string path)
        {
            var assembly = LoadPluginAsm(path);
            var plugins = assembly
                .GetTypes()
                .Where(type => typeof(IPlugin).IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as IPlugin)
                .Where(plugin => plugin is not null)
                .ToArray();

            if (!plugins.Any()) return plugins;
            
            var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            _log.Warning(
                $"Can't find any type which implements {nameof(IPlugin)} in {assembly} from {assembly.Location}.\n" +
                $"Available types: {availableTypes}");

            return plugins;
        }

        #endregion Methods
    }
}