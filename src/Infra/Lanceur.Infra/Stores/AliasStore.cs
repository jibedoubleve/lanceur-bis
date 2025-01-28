using Humanizer;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class AliasStore : IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly ILogger<AliasStore> _logger;

    #endregion

    #region Constructors

    public AliasStore(IServiceProvider serviceProvider)
    {
        _aliasRepository = serviceProvider.GetService<IAliasRepository>();
        _logger = serviceProvider.GetService<ILoggerFactory>()
                                 .GetLogger<AliasStore>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestration.SharedAlwaysActive();

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _aliasRepository.GetAll();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        var entry = _aliasRepository.Search(query.Name).ToArray();
        return entry;
    }

    #endregion
}