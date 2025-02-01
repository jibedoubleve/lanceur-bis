using Everything.Wrapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class EverythingStore : Store, IStoreService
{
    #region Fields

    private readonly IEverythingApi _everythingApi;
    private readonly ILogger<EverythingStore> _logger;
    private readonly ISettingsFacade _settings;

    private const string SearchAlias = ":";

    #endregion

    #region Constructors

    public EverythingStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger<EverythingStore>>();
        _everythingApi = serviceProvider.GetService<IEverythingApi>();
        _settings = serviceProvider.GetService<ISettingsFacade>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Exclusive(@"^\s{0,}:.*");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        if (query.Parameters.IsNullOrWhiteSpace()) return DisplayQueryResult.SingleFromResult("Enter text to search with Everything tool...");

        var result =  _everythingApi.Search(query.Parameters)
                                    .Select(item => item.ToAliasQueryResult())
                                    .Cast<QueryResult>()
                                    .ToList();
        return result;
    }

    #endregion
}