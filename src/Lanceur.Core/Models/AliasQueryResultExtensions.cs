namespace Lanceur.Core.Models;

public static class AliasQueryResultExtensions
{
    /// <summary>
    /// Adds new synonyms to the specified alias while ensuring no duplicates exist.
    /// If any synonyms already exist, they will be ignored.
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
    /// Adds a collection of additional parameters to the AliasQueryResult object.
    /// </summary>
    /// <param name="alias">The AliasQueryResult object to which the additional parameters will be added.</param>
    /// <param name="additionalParameters">A collection of QueryResultAdditionalParameters to add to the alias.</param>

    public static void AdditionalParameters(this AliasQueryResult alias, IEnumerable<AdditionalParameter> additionalParameters)
    {
        foreach (var additionalParameter in additionalParameters)
            alias.AdditionalParameters.Add(additionalParameter);
    }
}