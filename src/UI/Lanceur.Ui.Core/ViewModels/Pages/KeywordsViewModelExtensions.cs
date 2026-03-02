using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public static class KeywordsViewModelExtensions
{
    #region Methods

    /// <summary>
    ///     Reselects an alias from a list of aliases based on the ID and name of a given alias.
    /// </summary>
    /// <param name="aliases">The list of available <see cref="AliasQueryResult" />.</param>
    /// <param name="selectedAlias">The currently selected <see cref="AliasQueryResult" /> or null if no alias is selected.</param>
    /// <returns>
    ///     If <paramref name="selectedAlias" /> is null or has an invalid ID, returns the first alias from the list.
    ///     Otherwise, returns the alias from the list that matches both the ID and the name of the selected alias.
    /// </returns>
    private static AliasQueryResult? Reselect(this IList<AliasQueryResult> aliases, AliasQueryResult? selectedAlias)
    {
        if (aliases.Count == 0) { return null; }

        var id = selectedAlias?.Id ?? 0;
        var name = selectedAlias?.Synonyms.SplitCsv().FirstOrDefault();
        return id == 0
            ? aliases[0]
            : aliases.FirstOrDefault(e => e.Id == id && e.Name == name);
    }

    /// <summary>
    ///     Attempts to find and update a matching alias from the list based on the given source alias.
    /// </summary>
    /// <param name="aliases">The list of available <see cref="AliasQueryResult" /> instances.</param>
    /// <param name="src">
    ///     The currently selected <see cref="AliasQueryResult" />, or <c>null</c> if no alias is selected.
    /// </param>
    /// <returns>
    ///     Returns <c>null</c> if no matching alias is found. If <paramref name="src" /> is <c>null</c> or has an invalid ID,
    ///     the first alias from the list is returned. Otherwise, returns the alias that matches both the ID and name,
    ///     with its properties updated using the source alias.
    /// </returns>
    public static AliasQueryResult Hydrate(this IList<AliasQueryResult> aliases, AliasQueryResult? src)
    {
        var selected = aliases.Reselect(src)!;

        if (src is null) { return selected; }

        selected.Rehydrate(src);
        return selected;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="AdditionalParameter" /> class using the
    ///     <see cref="KeywordsViewModel.SelectedAlias" /> from the provided <paramref name="keywordsViewModel" />.
    ///     This method pre-fills the object with the AliasId based on the selected alias if available.
    /// </summary>
    /// <param name="keywordsViewModel">
    ///     The view model containing the selected alias to be used in creating the additional
    ///     parameter.
    /// </param>
    /// <returns>
    ///     A new <see cref="AdditionalParameter" /> object with the AliasId set from the selected alias, or null if no
    ///     alias is selected.
    /// </returns>
    public static AdditionalParameter? NewAdditionalParameter(this KeywordsViewModel keywordsViewModel)
    {
        if (keywordsViewModel.SelectedAlias is null) { return null; }

        return new() { AliasId = keywordsViewModel.SelectedAlias.Id };
    }

    #endregion
}