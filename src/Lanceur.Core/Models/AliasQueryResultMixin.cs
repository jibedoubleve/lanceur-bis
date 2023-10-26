namespace Lanceur.Core.Models;

public static class AliasQueryResultMixin
{
    #region Methods

    public static void UpdateIcon(this AliasQueryResult alias)
    {
        if (Uri.TryCreate(alias.FileName, UriKind.Absolute, out _))
        {
            alias.Icon = "Web";
        }
    }

    #endregion Methods
}