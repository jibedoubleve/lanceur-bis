using Everything.Wrapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

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
    public Orchestration Orchestration => Orchestration.Exclusive(@"^\s{0,}:.*");

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

        return _everythingApi.Search(query.Parameters)
                             .Select(
                                 item => new AliasQueryResult
                                 {
                                     Name = item.Name,
                                     FileName = item.Path,
                                     Icon = GetIcon(item.ResultType),
                                     Thumbnail = null,
                                     IsThumbnailDisabled = true,
                                     Count = -1
                                 }
                             )
                             .Cast<QueryResult>()
                             .ToList();
    }

    #endregion
}