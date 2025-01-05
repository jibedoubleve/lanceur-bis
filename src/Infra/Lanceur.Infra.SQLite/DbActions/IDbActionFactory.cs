namespace Lanceur.Infra.SQLite.DbActions;

public interface IDbActionFactory
{
    #region Properties

    AliasDbAction AliasManagement { get; }
    MacroDbAction MacroManagement { get; }
    AliasSaveDbAction SaveManagement { get; }
    AliasSearchDbAction SearchManagement { get; }
    SetUsageDbAction UsageManagement { get; }

    #endregion
}