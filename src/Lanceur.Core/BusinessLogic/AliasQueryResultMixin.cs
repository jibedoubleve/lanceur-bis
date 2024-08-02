using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.BusinessLogic;

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
        var items = string.Join(
            ',',
            alias.Synonyms
                 .Replace(' ', ',')
                 .Split(',', StringSplitOptions.RemoveEmptyEntries)
        );
        alias.Synonyms = items;
    }

    public static IEnumerable<AliasQueryResult> CloneFromSynonyms(this AliasQueryResult alias)
    {
        var names = alias.Synonyms.SplitCsv();
        var errors = new List<Exception>();
        var results = new List<AliasQueryResult>();

        foreach (var name in names)
            try
            {
                var toAdd = alias.CloneObject();
                toAdd.Name = name;
                results.Add(toAdd);
            }
            catch (Exception ex) { errors.Add(ex); }

        if (errors.Any()) throw new AggregateException($"Errors occured while updating synonyms of alias with these synonyms: {alias.Synonyms}", errors);

        return results.ToList();
    }

    #endregion Methods
}