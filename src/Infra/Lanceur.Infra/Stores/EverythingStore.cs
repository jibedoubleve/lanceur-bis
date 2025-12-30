using Everything.Wrapper;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class EverythingStore : Store, IStoreService
{
    #region Fields

    private readonly IEverythingApi _everythingApi;
    private readonly ILogger<EverythingStore> _logger;
    private readonly ISection<StoreSection> _settings;

    private const string SearchAlias = ":";

    #endregion

    #region Constructors

    public EverythingStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ILogger<EverythingStore> logger,
        IEverythingApi everythingApi,
        ISection<StoreSection> settings
    ) : base(orchestrationFactory)
    {
        _logger = logger;
        _everythingApi = everythingApi;
        _settings = settings;
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
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        if (cmdline.Parameters.IsNullOrWhiteSpace())
            return DisplayQueryResult.SingleFromResult("Enter text to search with Everything tool...");

        var query = $"{cmdline.Parameters} {_settings.Value.EverythingQuerySuffix}";
        _logger.LogTrace("Everything query: {Query}", query);

        var result =  _everythingApi.Search(query);

        if (result.HasError) return DisplayQueryResult.SingleFromResult(result.ErrorMessage);

        return result.ToList()
                     .Select(item => item.ToAliasQueryResult());
    }

    #endregion
}