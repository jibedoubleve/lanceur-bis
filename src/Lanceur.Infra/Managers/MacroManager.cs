using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Splat;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Managers
{
    public class MacroManager : IMacroManager
    {
        #region Fields

        private static Dictionary<string, SelfExecutableQueryResult> _macroInstances = null;
        private readonly Assembly _asm;
        private readonly IDataService _dataService;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public MacroManager(Assembly asm, IAppLoggerFactory logFactory = null)
        {
            _asm = asm;
            _log = Locator.Current.GetLogger<MacroManager>(logFactory);
            _dataService = Locator.Current.GetService<IDataService>();
        }

        #endregion Constructors

        #region Methods

        private void LoadMacros()
        {
            var types = _asm.GetTypes();
            if (_macroInstances is null)
            {
                var found = from t in types
                            where t.GetCustomAttributes<MacroAttribute>().Any()
                            select t;
                var macroInstances = new Dictionary<string, SelfExecutableQueryResult>();
                foreach (var type in found)
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance is SelfExecutableQueryResult alias)
                    {
                        var name = alias.Name = (type.GetCustomAttribute(typeof(MacroAttribute)) as MacroAttribute)?.Name;
                        name = name.ToUpper().Replace("@", string.Empty);

                        var description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                        alias.Description = description;

                        _dataService.HydrateMacro(alias);

                        macroInstances.Add(name, alias);
                        _log.Trace($"Found macro '{name}'");
                    }
                }
                _macroInstances = macroInstances;
            }
        }

        public IEnumerable<string> GetAll()
        {
            LoadMacros();
            return _macroInstances.Keys;
        }

        public IEnumerable<QueryResult> Handle(IEnumerable<QueryResult> collection)
        {
            LoadMacros();

            var results = new List<QueryResult>();
            foreach (var item in collection)
            {
                var toAdd = Handle(item);
                if (toAdd != null)
                {
                    results.Add(toAdd);
                }
            }

            return results;
        }

        public QueryResult Handle(QueryResult item)
        {
            if (item is AliasQueryResult src && src.IsMacro())
            {
                if (_macroInstances.ContainsKey(src.GetMacro()))
                {
                    var macro = _macroInstances[src.GetMacro()];
                    macro.Name = src.Name;
                    macro.Parameters = src.Parameters;
                    return macro;
                }
                else
                {
                    /* Well, this is a misconfigured macro, log it and forget it */
                    _log.Warning($"User has misconfigured a Macro with name '{src.FileName}'. Fix the name of the macro or remove the alias from the database.");
                    return null;
                }
            }
            else { return item; }
        }

        #endregion Methods
    }
}