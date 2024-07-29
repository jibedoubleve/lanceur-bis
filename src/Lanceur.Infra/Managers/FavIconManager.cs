using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Web;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Managers
{
    public class FavIconManager : IFavIconManager
    {
        #region Fields

        /// <summary>
        /// A regex to check whether the specified text
        /// is the template of a Macro
        /// </summary>
        private static readonly Regex IsMacroRegex = new("@.*@");

        private readonly IFavIconDownloader _favIconDownloader;
        private readonly ILogger<IFavIconManager> _logger;

        #endregion Fields

        #region Constructors

        public FavIconManager(IPackagedAppSearchService searchService, IFavIconDownloader favIconDownloader, ILoggerFactory appLoggerFactory)
        {
            ArgumentNullException.ThrowIfNull(searchService);
            ArgumentNullException.ThrowIfNull(favIconDownloader);
            ArgumentNullException.ThrowIfNull(appLoggerFactory);

            _favIconDownloader = favIconDownloader;
            _logger = appLoggerFactory.CreateLogger<FavIconManager>();
        }

        #endregion Constructors

        #region Methods

        public async Task RetrieveFaviconAsync(string fileName)
        {
            if (fileName is null) return;
            if (IsMacroRegex.Match(fileName).Success) return;
            if (!Uri.TryCreate(fileName, UriKind.Absolute, out var uri)) return;
            
            var output = Path.Combine(Paths.ImageRepository, $"{Paths.FaviconPrefix}{uri.Host}.png");
            if (File.Exists(output)) return;
            
            if (!await _favIconDownloader.CheckExistsAsync(new($"{uri.Scheme}://{uri.Host}"))) return;

            await _favIconDownloader.SaveToFileAsync(uri, output);
        }

        #endregion Methods
    }
}