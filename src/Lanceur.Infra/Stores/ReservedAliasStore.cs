using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Splat;
using System.ComponentModel;
using System.Reflection;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lanceur.Infra.Stores;

[Store]
public class ReservedAliasStore : IStoreService
{
    #region Fields

    private readonly Assembly _assembly;
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<ReservedAliasStore> _logger;
    private IEnumerable<QueryResult> _reservedAliases;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    /// <summary>
    /// Generate a new instance. Look into the Executing Assembly to find reserved aliases.
    /// </summary>
    /// <param name="serviceProvider">Service Provider used to inject dependencies</param>
    /// <remarks>
    /// Each reserved alias should be decorated with <see cref="ReservedAliasAttribute"/>
    /// </remarks>
    public ReservedAliasStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _assembly = serviceProvider.GetService<Assembly>();
        _dbRepository = serviceProvider.GetService<IDbRepository>();
        _logger = serviceProvider.GetService<ILogger<ReservedAliasStore>>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    #endregion

    #region Methods

    private void LoadAliases()
    {
        if (_reservedAliases != null) return;

        var types = _assembly.GetTypes();

        var found = types.Where(t => t.GetCustomAttributes<ReservedAliasAttribute>().Any())
                         .ToList();
        var foundItems = new List<QueryResult>();
        foreach (var type in found)
        {
            var instance = Activator.CreateInstance(type, _serviceProvider);

            if (instance is not SelfExecutableQueryResult qr) continue;

            var name = (type.GetCustomAttribute(typeof(ReservedAliasAttribute)) as ReservedAliasAttribute)?.Name;
            var keyword = _dbRepository.GetKeyword(name);

            qr.Name = name;
            if (keyword is not null)
            {
                qr.Id = keyword.Id;
                qr.Count = keyword.Count;
            }

            qr.Description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
            qr.Icon = "Settings24";

            foundItems.Add(qr);
        }

        _reservedAliases = foundItems;
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll()
    {
        if (_reservedAliases == null) LoadAliases();
        return _dbRepository.RefreshUsage(_reservedAliases);
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        var result = GetAll()
                     .Where(k => k.Name.ToLower().StartsWith(query.Name))
                     .ToList();

        var orderedResult = _dbRepository
                            .RefreshUsage(result)
                            .OrderByDescending(x => x.Count)
                            .ThenBy(x => x.Name);
        return orderedResult;
    }

    #endregion
}