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
        set => Set(ref _isSelected, value);
    }

    #endregion
}