using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public static class KeywordsViewModelHelpers
{
    #region Methods

    /// <summary>
    /// Reselects an alias from a list of aliases based on the ID and name of a given alias.
    /// </summary>
    /// <param name="aliases">The list of available <see cref="AliasQueryResult"/>.</param>
    /// <param name="selectedAlias">The currently selected <see cref="AliasQueryResult"/> or null if no alias is selected.</param>
    /// <returns>
    /// If <paramref name="selectedAlias"/> is null or has an invalid ID, returns the first alias from the list.
    /// Otherwise, returns the alias from the list that matches both the ID and name of the selected alias.
    /// </returns>
    public static AliasQueryResult? ReselectAlias(this IList<AliasQueryResult> aliases, AliasQueryResult? selectedAlias)
    {
        var id = selectedAlias?.Id ?? 0;
        var name = selectedAlias?.Synonyms.SplitCsv().FirstOrDefault();
        return id == 0
            ? aliases[0]
            : aliases.FirstOrDefault(e => e.Id == id && e.Name == name);
    }

    #endregion
}