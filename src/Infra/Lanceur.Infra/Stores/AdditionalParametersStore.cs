using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class AdditionalParametersStore : Store, IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasService;
    private readonly ILogger<AdditionalParametersStore> _logger;

    #endregion

    #region Constructors

    public AdditionalParametersStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _aliasService = serviceProvider.GetService<IAliasRepository>();
        _logger = serviceProvider.GetService<ILogger<AdditionalParametersStore>>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Shared(".*:.*");

    #endregion

    #region Methods

    /// <inheritdoc cref="IStoreService.GetAll"/>
    public override IEnumerable<QueryResult> GetAll() => _aliasService.GetAllAliasWithAdditionalParameters();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var _ = _logger.WarnIfSlow(this);
        return _aliasService.SearchAliasWithAdditionalParameters(cmdline.Name);
    }

    #endregion
}