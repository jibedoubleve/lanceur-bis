using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Splat;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Managers
{
    public abstract class MacroManagerCache
    {
        #region Fields

        private static Dictionary<string, SelfExecutableQueryResult> _macroInstances = null;
        private readonly Assembly _asm;
        private readonly IDbRepository _dataService;
        protected readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        internal MacroManagerCache(Assembly asm, IAppLoggerFactory logFactory)
        {
            _asm = asm;
            _log = Locator.Current.GetLogger<MacroManager>(logFactory);
            _dataService = Locator.Current.GetService<IDbRepository>();
        }

        #endregion Constructors

        #region Properties

        protected Dictionary<string, SelfExecutableQueryResult> MacroInstances
        {
            get
            {
                LoadMacros();
                return _macroInstances;
            }
        }

        #endregion Properties

        #region Methods

        private void LoadMacros()
        {
            if (_macroInstances is null)
            {
                var found = from t in _asm.GetTypes()
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

        #endregion Methods
    }
}