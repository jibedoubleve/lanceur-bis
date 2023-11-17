using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Models;

public static class AliasQueryResultMixin
{
    #region Methods

    /// <summary>
    /// Set first names defined in the synonyms as the name of the alias
    /// </summary>
    /// <param name="alias">The alias</param>
    public static void SetName(this AliasQueryResult alias)
    {
        alias.Name = alias.Synonyms
                          .SplitCsv()
                          .FirstOrDefault();
    }

    /// <summary>
    /// Indicates whether this alias is a packaged application (i.e: UWP)
    /// </summary>
    /// <param name="alias">The alias ti check</param>
    /// <returns><c>True</c> if this is a packaged application; otherwise <c>False</c></returns>
    public static bool IsPackagedApplication(this AliasQueryResult alias) 
        => alias.FileName.ToLower().StartsWith("package:");

    #endregion Methods
}