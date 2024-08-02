namespace Lanceur.Core.Models;

public abstract class MacroQueryResult : SelfExecutableQueryResult
{
    #region Methods

    public abstract SelfExecutableQueryResult Clone();

    #endregion Methods
}