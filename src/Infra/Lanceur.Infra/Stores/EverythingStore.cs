using Everything.Wrapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class EverythingStore : IStoreService
{
    #region Fields

    private readonly IEverythingApi _everythingApi;
    private readonly ILogger<EverythingStore> _logger;
    private readonly ISettingsFacade _settings;

    private const string SearchAlias = ":";

    #endregion

    #region Constructors

    public EverythingStore(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger<EverythingStore>>();
        _everythingApi = serviceProvider.GetService<IEverythingApi>();
        _settings = serviceProvider.GetService<ISettingsFacade>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestration.Exclusive(@"^\s{0,}:.*");

    #endregion

    #region Methods

    private static string GetIcon(ResultType itemResultType) => itemResultType switch
    {
        ResultType.File       => "FileOutline",
        ResultType.Excel      => "FileExcelOutline",
        ResultType.Pdf        => "FilePdfBox",
        ResultType.Zip        => "ZipBoxOutline",
        ResultType.Image      => "FileImageOutline",
        ResultType.Word       => "FileWordBoxOutline",
        ResultType.Directory  => "FolderOutline",
        ResultType.Music      => "FileMusicOutline",
        ResultType.Text       => "FileDocumentOutline",
        ResultType.Code       => "FileCodeOutline",
        ResultType.Executable => "FileCogOutline",
        _                     => "HelpCircleOutline"
    };

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        if (query.Name != SearchAlias) return Array.Empty<QueryResult>();

        var result =  _everythingApi.Search(query.Parameters)
                                    .Select(
                                        item =>
                                        {
                                            var qr = new AliasQueryResult
                                            {
                                                Name = item.Name,
                                                FileName = item.Path,
                                                Icon = GetIcon(item.ResultType),
                                                Thumbnail = null,
                                                IsThumbnailDisabled = true
                                            };
                                            new QueryResultCounterIncrement(qr).SetCount(-1);
                                            return qr;
                                        }
                                    )
                                    .Cast<QueryResult>()
                                    .ToList();
        _logger.LogTrace("Found {Count} results for request {Request}", result.Count, query);
        return result;
    }

    #endregion
}