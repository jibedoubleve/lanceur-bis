namespace Lanceur.Infra.SQLite.DbActions;

public interface IDbActionFactory
{
    #region Methods

    AliasDbAction AliasDbAction();
    AliasSearchDbAction AliasSearchDbAction();
    MacroDbAction MacroDbAction();
    SetUsageDbAction SetUsageDbAction();

    #endregion
}