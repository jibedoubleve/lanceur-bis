using System.ComponentModel;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public abstract class MacroCachedService
{
    #region Fields

    private readonly Assembly _asm;
    private readonly IAliasRepository _aliasRepository;
    private Dictionary<string, ISelfExecutable> _macroTemplates;
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

    protected Dictionary<string, ISelfExecutable> MacroTemplates
    {
        get
        {
            LoadMacros();
            return _macroTemplates;
        }
    }

    #endregion

    #region Methods

    private void LoadMacros()
    {
        if (_macroTemplates is not null) return;

        var found = _asm.GetTypes()
                        .Where(t => t.GetCustomAttributes<MacroAttribute>().Any());

        var macroInstances = new Dictionary<string, ISelfExecutable>();
        foreach (var type in found)
        {
            var instance = Activator.CreateInstance(type, _serviceProvider);
            if (instance is not SelfExecutableQueryResult alias) continue;

            var name = alias.Name = (type.GetCustomAttribute(typeof(MacroAttribute)) as MacroAttribute)?.Name;
            name = name?.ToUpper().Replace("@", string.Empty) ?? string.Empty;

            var description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
            alias.Description = description;

            _aliasRepository.HydrateMacro(alias);

            macroInstances.Add(name, alias);
            Logger.LogDebug("Found macro {Name}", name);
        }

        _macroTemplates = macroInstances;
    }

    #endregion
}