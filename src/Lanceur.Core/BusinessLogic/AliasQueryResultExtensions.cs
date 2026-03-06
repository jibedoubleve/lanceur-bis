using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.BusinessLogic;

public static class AliasQueryResultExtensions
{
    #region Methods

    public static bool IsUwp(this AliasQueryResult alias) => alias.FileName.IsUwp();

    /// <summary>
    ///     Clears all the useless quotes in the filename
    /// </summary>
    /// <param name="alias">The <see cref="AliasQueryResult" /> to sanitize</param>
    public static void SanitizeFileName(this AliasQueryResult alias)
    {
        if (alias?.FileName is null) { return; }

        alias.FileName = alias.FileName.Replace("\"", string.Empty);
    }

    /// <summary>
    ///     Clears all the useless spaces and comas
    /// </summary>
    /// <param name="alias">The <see cref="AliasQueryResult" /> to sanitize</param>
    public static void SanitizeSynonyms(this AliasQueryResult alias)
    {
        var items = alias.Synonyms.IsNullOrEmpty()
            ? null
            : string.Join(
                ',',
                alias.Synonyms!
                     .Replace(' ', ',')
                     .Split(',', StringSplitOptions.RemoveEmptyEntries)
            );
        alias.Synonyms = items;
    }

    #endregion
}