using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using Lanceur.Infra.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Managers;

public abstract class MacroCachedService
{
    #region Fields

    private readonly Assembly _asm;
    private readonly IAliasRepository _aliasRepository;
    private Dictionary<string, ISelfExecutable> _macroInstances;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    internal MacroCachedService(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _asm = serviceProvider.GetService<AssemblySource>().MacroSource;
        Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MacroService>();
        _aliasRepository = serviceProvider.GetService<IAliasRepository>();
        _serviceProvider = serviceProvider;
    }

    #endregion

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

    #endregion

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

            _aliasRepository.HydrateMacro(alias);

            macroInstances.Add(name, alias);
            Logger.LogDebug("Found macro {Name}", name);
        }

        _macroInstances = macroInstances;
    }

    #endregion
}