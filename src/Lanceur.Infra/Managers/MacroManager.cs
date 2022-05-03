using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Splat;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Managers
{
    public class MacroManager : IMacroManager
    {
        #region Fields

        private static Dictionary<string, ExecutableQueryResult> _macroInstances = null;
        private readonly Assembly _asm;
        private readonly ILogService _log;

        #endregion Fields

        #region Constructors

        public MacroManager(Assembly asm, ILogService log = null)
        {
            _asm = asm;
            _log = log ?? Locator.Current.GetService<ILogService>() ?? new TraceLogService();
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
                var macroInstances = new Dictionary<string, ExecutableQueryResult>();
                foreach (var type in found)
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance is ExecutableQueryResult alias)
                    {
                        var name = alias.Name = (type.GetCustomAttribute(typeof(MacroAttribute)) as MacroAttribute)?.Name;
                        name = name.ToUpper().Replace("@", string.Empty);

                        var description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                        alias.SetDescription(description);

                        macroInstances.Add(name, alias);
                        _log.Info($"Found macro '{name}'");
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
                if (item is AliasQueryResult src && src.IsMacro())
                {
                    if (_macroInstances.ContainsKey(src.GetMacro()))
                    {
                        var macro = _macroInstances[src.GetMacro()];
                        macro.Name = src.Name;
                        results.Add(macro);
                    }
                    else
                    {
                        /* Well, this a misconfigured macro, log it and forget it */
                        _log.Warning($"User has misconfigured a Macro with name '{src.FileName}'. Fix the name of the macro or remove the alias from the database.");
                    }
                }
                else { results.Add(item); }
            }

            return results;
        }

        #endregion Methods
    }
}