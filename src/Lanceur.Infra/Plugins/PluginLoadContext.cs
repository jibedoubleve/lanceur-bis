using System.Reflection;
using System.Runtime.Loader;

namespace Lanceur.Infra.Plugins
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        #region Fields

        private readonly AssemblyDependencyResolver _resolver;

        #endregion Fields

        #region Constructors

        public PluginLoadContext(string pluginPath)
        {
            ArgumentNullException.ThrowIfNull(pluginPath, nameof(pluginPath));

            if (!File.Exists(pluginPath))
            {
                throw new ArgumentException($"Cannot load plugin. Specified path doesn't exist. (Path: '{pluginPath}')", nameof(pluginPath));
            }
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        #endregion Constructors

        #region Methods

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var asmPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return asmPath != null
                ? LoadFromAssemblyPath(asmPath)
                : null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return libraryPath != null
                ? LoadUnmanagedDllFromPath(libraryPath)
                : IntPtr.Zero;
        }

        #endregion Methods
    }
}