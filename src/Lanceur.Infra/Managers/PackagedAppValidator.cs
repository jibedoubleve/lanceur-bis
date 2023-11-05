using System.ComponentModel.DataAnnotations;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Web;

namespace Lanceur.Infra.Managers
{
    public class PackagedAppValidator : IPackagedAppValidator
    {
        #region Fields

        private readonly IPackagedAppSearchService _searchService;
        private readonly IFavIconDownloader _favIconDownloader;

        #endregion Fields

        #region Constructors

        public PackagedAppValidator(IPackagedAppSearchService searchService, IFavIconDownloader favIconDownloader)
        {
            ArgumentNullException.ThrowIfNull(searchService);
            ArgumentNullException.ThrowIfNull(favIconDownloader);
            
            _searchService = searchService;
            _favIconDownloader = favIconDownloader;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Checks whether the alias is a packaged app. If it's the case,
        /// it'll fix the <see cref="AliasQueryResult.FileName"/> and the <see cref="QueryResult.Icon"/>
        /// of the specified alias
        /// </summary>
        /// <param name="alias">The alias to standardise</param>
        /// <returns>Standardised alias</returns>
        public async Task<AliasQueryResult> FixAsync(AliasQueryResult alias)
        {
            if (alias is null) return default;

            var response = await Task.Run(() => _searchService.GetByInstalledDirectory(alias.FileName)
                                                              .FirstOrDefault());

            if (response is null)
            {
                var uri = new Uri(alias.FileName);

                if (!await _favIconDownloader.CheckExistsAsync(new($"{uri.Scheme}://{uri.Host}"))) return alias;
                
                var output = Path.Combine(AppPaths.ImageCache.ExpandPath(), $"{AppPaths.FaviconPrefix}{uri.Host}.png");
                await _favIconDownloader.SaveToFileAsync(uri, output);
                alias.Thumbnail = output;
                alias.Icon = null;
                return alias;
            }

            // This is a packaged app
            alias.FileName = $"package:{response.AppUserModelId}";
            alias.Thumbnail = response.Logo.LocalPath;
            alias.Icon = null;
            return alias;
        }

        #endregion Methods
    }
}