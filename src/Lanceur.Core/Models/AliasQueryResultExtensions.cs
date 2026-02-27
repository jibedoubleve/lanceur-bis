using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public static class AliasQueryResultExtensions
{
    #region Methods

    /// <summary>
    ///     Adds new synonyms to the specified alias while ensuring no duplicates exist.
    ///     If any synonyms already exist, they will be ignored.
    /// </summary>
    /// <param name="alias">The alias object to update with new synonyms</param>
    /// <param name="synonyms">A collection of new synonyms to be added</param>
    public static void AddDistinctSynonyms(this AliasQueryResult alias, IEnumerable<string> synonyms)
    {
        synonyms ??= new List<string>();
        var lis = synonyms.ToList();
        
        lis.Add(alias.Synonyms);
        var aggregation = string.Join(", ", lis)
                           .Split(",")
                           .Select(x => x.Trim())
                           .Distinct()
                           .ToArray();
        
        alias.Synonyms = string.Join(",", aggregation);
    }

    /// <summary>
    ///     Indicates whether this alias is a packaged application (i.e: UWP)
    /// </summary>
    /// <param name="alias">The alias ti check</param>
    /// <returns><c>True</c> if this is a packaged application; otherwise <c>False</c></returns>
    public static bool IsPackagedApplication(this AliasQueryResult alias)
    {
        return !alias.FileName.IsNullOrEmpty() 
               && alias.FileName.StartsWith("package:", StringComparison.CurrentCultureIgnoreCase);
    }

    #endregion
}