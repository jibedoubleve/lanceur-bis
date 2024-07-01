using Everything.Wrapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Stores;

[Store]
public class EverythingStore : ISearchService
{
    private readonly ISettingsFacade _settings;

    #region Fields

    private const string SearchAlias = ":";
    private readonly IEverythingApi _everythingApi;
    private readonly ILogger<EverythingStore> _logger;

    #endregion Fields

    #region Constructors

    public EverythingStore() : this(null) { }

    public EverythingStore(ILoggerFactory loggerFactory = null, IEverythingApi everythingApi = null, ISettingsFacade settings = null)
    {
        var l = Locator.Current;
        _everythingApi = everythingApi ?? new EverythingApi();
        _settings = settings ?? l.GetService<ISettingsFacade>();

        var factory = loggerFactory ?? l.GetService<ILoggerFactory>();
        _logger = factory.GetLogger<EverythingStore>();
    }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.Exclusive(@"^\s{0,}:.*");

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        if (query.Name != SearchAlias) return Array.Empty<QueryResult>();

        return _everythingApi.Search(query.Parameters, _settings.Application.EverythingResultsShowIcon)
                             .Select(item => new AliasQueryResult
                             {
                                 Name = item.Name,
                                 FileName = item.Path,
                                 Icon = GetIcon(item.ResultType),
                                 Thumbnail = null,
                                 IsThumbnailDisabled = true,
                                 Count = -1
                             }).Cast<QueryResult>()
                             .ToList();
    }
    
    private string GetIcon(ResultType itemResultType) => itemResultType switch
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

    #endregion Methods
}