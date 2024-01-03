using Lanceur.Core.Plugins;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using System.Reflection;

namespace Lanceur.Infra.Plugins
{
    public class PluginManager : IPluginManager
    {
        #region Fields

        private readonly ILogger<PluginManager> _logger;

        #endregion Fields

        #region Constructors

        public PluginManager(ILoggerFactory logFactory = null)
        {
            _logger = logFactory.GetLogger<PluginManager>();
        }

        #endregion Constructors

        #region Methods

        private Assembly LoadPluginAsm(string relativePath)
        {
            var loc = Locations.FromFile(relativePath).PluginDllPath;

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
            _logger.LogWarning(
                "Can't find any type which implements {IPlugin} in {Assembly} from {Location}. Available types: {AvailableTypes}",
                nameof(IPlugin),
                assembly,
                assembly.Location,
                availableTypes);

            return plugins;
        }

        #endregion Methods
    }
}