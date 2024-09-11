using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Managers;

public abstract class MacroManagerCache
{
    #region Fields

    private readonly Assembly _asm;
    private readonly IDbRepository _dbRepository;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, ISelfExecutable> _macroInstances;

    #endregion Fields

    #region Constructors

    internal MacroManagerCache(Assembly asm, ILogger logger, IDbRepository dbRepository, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(asm);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbRepository);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        
        _asm = asm;
        Logger = logger;
        _dbRepository = dbRepository;
        _serviceProvider = serviceProvider;
    }

    #endregion Constructors

    #region Properties

    protected ILogger Logger { get; }

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

        var found = _asm.GetTypes()
                        .Where(t => t.GetCustomAttributes<MacroAttribute>().Any());

        var macroInstances = new Dictionary<string, ISelfExecutable>();
        foreach (var type in found)
        {
            var instance = Activator.CreateInstance(type, [_serviceProvider]);
            if (instance is not SelfExecutableQueryResult alias) continue;

            var name = alias.Name = (type.GetCustomAttribute(typeof(MacroAttribute)) as MacroAttribute)?.Name;
            name = name?.ToUpper().Replace("@", string.Empty) ?? string.Empty;

            var description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
            alias.Description = description;

            _dbRepository.HydrateMacro(alias);

            macroInstances.Add(name, alias);
            Logger.LogDebug("Found macro {Name}", name);
        }

        _macroInstances = macroInstances;
    }

    #endregion Methods
}