namespace Lanceur.Core.Models;

public class SelectableAliasQueryResult : AliasQueryResult
{
    #region Fields

    private bool _isSelected;

    #endregion

    #region Properties

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    #endregion
}