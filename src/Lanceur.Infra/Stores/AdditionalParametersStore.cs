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
public class AdditionalParametersStore : IStoreService
{
    #region Fields

    private readonly IDbRepository _aliasService;
    private readonly ILogger<AdditionalParametersStore> _logger;

    #endregion

    #region Constructors

    public AdditionalParametersStore(IServiceProvider serviceProvider)
    {
        _aliasService = serviceProvider.GetService<IDbRepository>();
        _logger = serviceProvider.GetService<ILogger<AdditionalParametersStore>>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public Orchestration Orchestration { get; } = Orchestration.Shared(".*:.*");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _aliasService.GetAllAliasWithAdditionalParameters();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        return _aliasService.SearchAliasWithAdditionalParameters(query.Name);
    }

    #endregion
}