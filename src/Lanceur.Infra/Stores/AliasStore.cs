using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Stores;

[Store]
public class AliasStore : IStorehService
{
    #region Fields

    private readonly IDbRepository _aliasService;
    private readonly ILogger<AliasStore> _logger;

    #endregion Fields

    #region Constructors

    public AliasStore(IServiceProvider serviceProvider)
    {
        _aliasService = serviceProvider.GetService<IDbRepository>();
        _logger = serviceProvider.GetService<ILogger<AliasStore>>();
    }

    [Obsolete("Use ctor with service provider instead")]
    public AliasStore(IDbRepository aliasService = null, ILoggerFactory loggerFactory = null)
    {
        _aliasService = aliasService ?? Locator.Current.GetService<IDbRepository>();

        loggerFactory ??= Locator.Current.GetService<ILoggerFactory>();
        _logger = loggerFactory.GetLogger<AliasStore>();
    }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _aliasService.GetAll();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        return _aliasService.Search(query.Name);
    }

    #endregion Methods
}