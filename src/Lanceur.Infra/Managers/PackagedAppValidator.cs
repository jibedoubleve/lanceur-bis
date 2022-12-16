using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Managers
{
    public class PackagedAppValidator : IPackagedAppValidator
    {
        #region Fields

        private readonly IPackagedAppManager _packagedAppManager;

        #endregion Fields

        #region Constructors

        public PackagedAppValidator(IPackagedAppManager packagedAppManager)
        {
            _packagedAppManager = packagedAppManager;
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
            var response = await _packagedAppManager.GetPackageInfoAsync(alias.FileName);
            if (response.IsPackage)
            {
                alias.FileName = response.Uri;
                alias.Icon = await _packagedAppManager.GetIconAsync(response.FileName);
            }
            return alias;
        }

        #endregion Methods
    }
}