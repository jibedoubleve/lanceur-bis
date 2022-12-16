using Lanceur.Core.Models;

namespace Lanceur.Infra.Managers
{
    public interface IPackagedAppValidator
    {
        #region Methods
        /// <summary>
        /// Checks whether the alias is a packaged app. If it's the case,
        /// it'll fix the <see cref="AliasQueryResult.FileName"/> and the <see cref="QueryResult.Icon"/>
        /// of the specified alias
        /// </summary>
        /// <param name="alias">The alias to standardise</param>
        /// <returns>Standardised alias</returns>
        Task<AliasQueryResult> FixAsync(AliasQueryResult alias);

        #endregion Methods
    }
}