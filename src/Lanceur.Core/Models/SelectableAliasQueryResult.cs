namespace Lanceur.Core.Models;

public class SelectableAliasQueryResult : AliasQueryResult
{
    #region Properties

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion
}