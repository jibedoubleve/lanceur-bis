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

    #endregion Methods
}