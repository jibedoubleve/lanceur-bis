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
        var aggregation = alias.Synonyms.Split(",").Select(e => e.Trim()).ToList();
        aggregation.AddRange(synonyms);
        alias.Synonyms = string.Join(",", aggregation.Distinct());
    }

    /// <summary>
    ///     Adds a collection of additional parameters to the AliasQueryResult object.
    /// </summary>
    /// <param name="alias">The AliasQueryResult object to which the additional parameters will be added.</param>
    /// <param name="additionalParameters">A collection of QueryResultAdditionalParameters to add to the alias.</param>
    public static void AdditionalParameters(this AliasQueryResult alias, IEnumerable<AdditionalParameter> additionalParameters)
    {
        foreach (var additionalParameter in additionalParameters) alias.AdditionalParameters.Add(additionalParameter);
    }

    /// <summary>
    ///     Indicates whether this alias is a packaged application (i.e: UWP)
    /// </summary>
    /// <param name="alias">The alias ti check</param>
    /// <returns><c>True</c> if this is a packaged application; otherwise <c>False</c></returns>
    public static bool IsPackagedApplication(this AliasQueryResult alias) => alias.FileName.ToLower().StartsWith("package:");

    /// <summary>
    ///     Set first names defined in the synonyms as the name of the alias
    /// </summary>
    /// <param name="alias">The alias</param>
    public static void SetName(this AliasQueryResult alias)
    {
        alias.Name = alias.Synonyms
                          .SplitCsv()
                          .FirstOrDefault();
    }

    #endregion
}