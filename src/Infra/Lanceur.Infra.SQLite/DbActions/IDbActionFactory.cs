namespace Lanceur.Infra.SQLite.DbActions;

public interface IDbActionFactory
{
    #region Methods

    AliasDbAction AliasDbAction();
    AliasSaveDbAction AliasSaveDbAction();
    AliasSearchDbAction AliasSearchDbAction();
    MacroDbAction MacroDbAction();
    SetUsageDbAction SetUsageDbAction();

    #endregion
}