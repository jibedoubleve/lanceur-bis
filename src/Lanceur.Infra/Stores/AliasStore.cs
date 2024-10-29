using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class AliasStore : IStoreService
{
    #region Fields

    private readonly IDbRepository _dbRepository;
    private readonly ILogger<AliasStore> _logger;

    #endregion

    #region Constructors

    public AliasStore(IServiceProvider serviceProvider)
    {
        _dbRepository = serviceProvider.GetService<IDbRepository>();
        _logger = serviceProvider.GetService<ILoggerFactory>()
                                 .GetLogger<AliasStore>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _dbRepository.GetAll();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        return _dbRepository.Search(query.Name);
    }

    #endregion
}