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

        private Dictionary<string, ISelfExecutable> _macroInstances;
        private readonly Assembly _asm;
        private readonly IDbRepository _dataService;

        #endregion Fields

        #region Constructors

        internal MacroManagerCache(Assembly asm, IAppLoggerFactory logFactory, IDbRepository repository)
        {
            _asm = asm;
            Log = Locator.Current.GetLogger<MacroManager>(logFactory);
            _dataService = repository ?? Locator.Current.GetService<IDbRepository>();
        }

        #endregion Constructors

        #region Properties

        protected IAppLogger Log { get;  }

        protected Dictionary<string, ISelfExecutable> MacroInstances
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
            if (_macroInstances is not null) return;

            var found = from t in _asm.GetTypes()
                        where t.GetCustomAttributes<MacroAttribute>().Any()
                        select t;
            var macroInstances = new Dictionary<string, ISelfExecutable>();
            foreach (var type in found)
            {
                var instance = Activator.CreateInstance(type);
                if (instance is not SelfExecutableQueryResult alias) continue;

                var name = alias.Name = (type.GetCustomAttribute(typeof(MacroAttribute)) as MacroAttribute)?.Name;
                name = name?.ToUpper().Replace("@", string.Empty) ?? string.Empty;

                var description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                alias.Description = description;

                _dataService.HydrateMacro(alias);

                macroInstances.Add(name, alias);
                Log.Trace("Found macro '{name}'", name);
            }
            _macroInstances = macroInstances;
        }

        #endregion Methods
    }
}