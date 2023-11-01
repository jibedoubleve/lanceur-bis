using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Managers
{
    public class PackagedAppValidator : IPackagedAppValidator
    {
        #region Fields

        private readonly IPackagedAppSearchService _searchService;

        #endregion Fields

        #region Constructors

        public PackagedAppValidator(IPackagedAppSearchService searchService) => _searchService = searchService;

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

            if (response is null) return alias;

            alias.FileName = $"package:{response.AppUserModelId}";
            alias.Thumbnail = response.Logo.LocalPath;
            alias.Icon = null;
            return alias;
        }

        #endregion Methods
    }
}