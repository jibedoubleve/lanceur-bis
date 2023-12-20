using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Web;
using System.Text.RegularExpressions;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Infra.Managers
{
    public class FavIconManager : IFavIconManager
    {
        #region Fields

        /// <summary>
        /// A regex to check whether the specified text
        /// is the template of a Macro
        /// </summary>
        private static readonly Regex IsMacro = new("@.*@");

        private readonly IFavIconDownloader _favIconDownloader;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public FavIconManager(IPackagedAppSearchService searchService, IFavIconDownloader favIconDownloader, IAppLoggerFactory appLoggerFactory)
        {
            ArgumentNullException.ThrowIfNull(searchService);
            ArgumentNullException.ThrowIfNull(favIconDownloader);
            ArgumentNullException.ThrowIfNull(appLoggerFactory);

            _favIconDownloader = favIconDownloader;
            _log = appLoggerFactory.GetLogger<FavIconManager>();
        }

        #endregion Constructors

        #region Methods

        public async Task RetrieveFaviconAsync(string fileName)
        {
            if (fileName is null) return;
            if (IsMacro.Match(fileName).Success) return;
            if (!Uri.TryCreate(fileName, UriKind.Absolute, out var uri)) return;
            if (!await _favIconDownloader.CheckExistsAsync(new($"{uri.Scheme}://{uri.Host}"))) return;

            using var m = TimePiece.Measure(this, m => _log.Trace(m));
            var output = Path.Combine(AppPaths.ImageRepository, $"{AppPaths.FaviconPrefix}{uri.Host}.png");
            await _favIconDownloader.SaveToFileAsync(uri, output);
        }

        #endregion Methods
    }
}