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
public class ReservedAliasStore : ISearchService
{
    #region Fields

    private readonly Assembly _assembly;
    private readonly IDbRepository _dataService;
    private readonly ILogger<ReservedAliasStore> _logger;
    private IEnumerable<QueryResult> _reservedAliases;

    #endregion Fields

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
        _assembly = Assembly.GetEntryAssembly();
        _dataService =serviceProvider.GetService<IDbRepository>();
        _logger = serviceProvider.GetService<ILogger<ReservedAliasStore>>();

    }

    /// <summary>
    /// Generate a new instance
    /// </summary>
    /// <param name="assembly">The assembly where to search the reserved aliases. </param>
    /// <param name="dataService">The service used to update usage of the alias</param>
    /// <param name="loggerFactory">Logger factory used to create logger</param>
    /// <remarks>
    /// Each reserved alias should be decorated with <see cref="ReservedAliasAttribute"/>
    /// </remarks>
    [Obsolete("Use ctor with service provider instead")]
    public ReservedAliasStore(Assembly assembly, IDbRepository dataService = null, ILoggerFactory loggerFactory = null)
    {
        _assembly = assembly;
        _dataService = dataService ?? Locator.Current.GetService<IDbRepository>();

        loggerFactory ??= Locator.Current.GetService<ILoggerFactory>();
        _logger = loggerFactory?.GetLogger<ReservedAliasStore>() ?? new NullLogger<ReservedAliasStore>();
    }

    #endregion Constructors

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
            var instance = Activator.CreateInstance(type);

            if (instance is not SelfExecutableQueryResult qr) continue;

            var name = (type.GetCustomAttribute(typeof(ReservedAliasAttribute)) as ReservedAliasAttribute)?.Name;
            var keyword = _dataService.GetKeyword(name);

            qr.Name = name;
            if (keyword is not null)
            {
                qr.Id = keyword.Id;
                qr.Count = keyword.Count;
            }

            qr.Description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
            qr.Icon = "ApplicationCogOutline";

            foundItems.Add(qr);
        }

        _reservedAliases = foundItems;
    }

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll()
    {
        if (_reservedAliases == null) LoadAliases();
        return _dataService.RefreshUsage(_reservedAliases);
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        var result = GetAll().Where(k => k.Name.ToLower().StartsWith(query.Name))
                             .ToList();

        var orderedResult = _dataService
                            .RefreshUsage(result)
                            .OrderByDescending(x => x.Count)
                            .ThenBy(x => x.Name);
        return orderedResult;
    }

    #endregion Methods
}