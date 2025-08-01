using System.Text;

namespace Lanceur.Tests.Tools.SQL;

public abstract class SqlGeneratorBase
{
    #region Fields

    protected readonly StringBuilder Sql = new();

    #endregion

    #region Constructors

    protected SqlGeneratorBase() { }

    protected SqlGeneratorBase(SqlGeneratorBase builder) => Sql = builder.Sql;

    #endregion

    #region Methods

    public string ToSql() => Sql.ToString();

    #endregion
}