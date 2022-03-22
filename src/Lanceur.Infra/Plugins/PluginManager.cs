using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Splat;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Core.Plugins
{
    public class PluginManager : IPluginManager
    {
        private readonly ILogService _log;

        public PluginManager()
        {
            _log = Locator.Current.GetService<ILogService>() ?? new TraceLogService();
        }
        #region Methods

        public QueryResult Activate(Type plugin)
        {
            var instance = Activator.CreateInstance(plugin);
            var name = (plugin.GetCustomAttribute(typeof(PluginAttribute)) as PluginAttribute)?.Name;
            var description = (plugin.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;

            if (instance is ExecutableQueryResult queryResult)
            {                
                queryResult.Icon = "PowerPlugOutline";
                queryResult.Name = name;
                queryResult.SetDescription(description);
                return queryResult;
            }
            else { return null; }
        }

        public bool Exists(string dll) => File.Exists(dll);

        public IEnumerable<Type> GetPluginTypes(string dll)
        {
            var asm = Assembly.LoadFile(dll);
            return File.Exists(dll)
                ? GetPluginTypes(Assembly.LoadFile(dll))
                : throw new FileNotFoundException("The plugin dll to load does not exist.", dll);
        }

        public IEnumerable<Type> GetPluginTypes(Assembly asm)
        {
            var results = new List<Type>();
            try
            {
                var types = from t in asm.GetTypes()
                            where t.GetCustomAttributes<PluginAttribute>().Any()
                            select t;
                results.AddRange(types);
            }
            catch (Exception ex) { _log.Error(ex.Message, ex); }

            return results;
        }

        #endregion Methods
    }
}