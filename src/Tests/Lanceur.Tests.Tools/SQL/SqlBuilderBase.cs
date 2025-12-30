using System.Text;

namespace Lanceur.Tests.Tools.SQL;

public abstract class SqlBuilderBase : ISqlBuilder
{
    #region Fields

    protected StringBuilder Sql = new();

    #endregion

    #region Methods

    public string ToSql() => Sql.ToString();

    #endregion
}