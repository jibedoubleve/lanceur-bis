﻿using System.ComponentModel;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class ReservedAliasStore : Store, IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    private readonly Assembly _assembly;
    private readonly ILogger<ReservedAliasStore> _logger;
    private IEnumerable<QueryResult> _reservedAliases;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    /// <summary>
    ///     Generate a new instance. Look into the Executing Assembly to find reserved aliases.
    /// </summary>
    /// <param name="serviceProvider">Service Provider used to inject dependencies</param>
    /// <remarks>
    ///     Each reserved alias should be decorated with <see cref="ReservedAliasAttribute" />
    /// </remarks>
    public ReservedAliasStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _assembly = serviceProvider.GetService<AssemblySource>()?.ReservedKeywordSource ?? throw new NullReferenceException("The AssemblySource is not set in the DI container.");
        _aliasRepository = serviceProvider.GetService<IAliasRepository>();
        _logger = serviceProvider.GetService<ILoggerFactory>()
                                 .GetLogger<ReservedAliasStore>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.SharedAlwaysActive();

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
            _aliasRepository.SetHiddenAliasUsage(qr);

            qr.Name = name;
            qr.Description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
            qr.Icon = "Settings24";

            foundItems.Add(qr);
        }

        _reservedAliases = foundItems;
    }

    private static IEnumerable<QueryResult> RefreshCounters(List<QueryResult> result, Dictionary<string, (long Id, int Count)> counters)
    {
        var orderedResult = result.Select(
            alias =>
            {
                if (counters is null) return alias;

                var r = counters.Where(counter => counter.Key == alias.Name)
                                .Select(a => a.Value)
                                .FirstOrDefault();
                alias.Id = r.Id;
                alias.Count = r.Count;
                return alias;
            }
        );
        return orderedResult;
    }

    /// <inheritdoc cref="IStoreService.GetAll"/>
    public override IEnumerable<QueryResult> GetAll()
    {
        if (_reservedAliases == null) LoadAliases();
        return _reservedAliases;
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var _ = _logger.WarnIfSlow(this);
        var result = GetAll()
                     .Where(k => k.Name.ToLower().StartsWith(cmdline.Name))
                     .ToList();

        var counters = _aliasRepository.GetHiddenCounters();
        return RefreshCounters(result, counters)
               .OrderByDescending(x => x.Count)
               .ThenBy(x => x.Name);
    }

    #endregion
}