using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Models;

public static class AliasQueryResultMixin
{
    #region Methods

    public static void UpdateIcon(this AliasQueryResult alias)
    {
        if (alias is null) return;

        var uri = alias.FileName ?? string.Empty;
        if (Uri.TryCreate(uri, UriKind.Absolute, out _)
            && uri.StartsWith("http"))
        {
            alias.Icon = "Web";
        }
    }

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

    #endregion Methods
}