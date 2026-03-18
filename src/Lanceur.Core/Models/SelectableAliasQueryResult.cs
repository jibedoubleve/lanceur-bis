namespace Lanceur.Core.Models;

public sealed class SelectableAliasQueryResult : AliasQueryResult
{
    #region Properties

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion
}