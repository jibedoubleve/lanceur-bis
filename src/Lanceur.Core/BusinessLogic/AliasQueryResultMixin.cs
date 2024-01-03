using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.BusinessLogic
{
    public static class AliasQueryResultMixin
    {
        #region Methods

        public static bool IsUwp(this AliasQueryResult alias) => alias.FileName.IsUwp();

        /// <summary>
        /// Clears all the useless spaces and comas
        /// </summary>
        /// <param name="alias">The <see cref="AliasQueryResult"/> to sanitize</param>
        public static void SanitizeSynonyms(this AliasQueryResult alias)
        {
            var items = string.Join(',',
                                    alias.Synonyms
                                         .Replace(' ', ',')
                                         .Split(',',StringSplitOptions.RemoveEmptyEntries)
                                );
            alias.Synonyms = items;
        }
        #endregion Methods
    }
}