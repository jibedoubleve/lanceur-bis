using System.Data;
using Lanceur.Infra.SQLite.DbActions;

namespace Lanceur.Infra.SQLite;

public interface IDbActionFactory
{
    #region Methods

    AliasDbAction AliasDbAction();
    AliasSearchDbAction AliasSearchDbAction();
    MacroDbAction MacroDbAction();
    SetUsageDbAction SetUsageDbAction();

    #endregion
}