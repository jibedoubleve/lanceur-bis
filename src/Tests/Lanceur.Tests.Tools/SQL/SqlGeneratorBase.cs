using System.Text;

namespace Lanceur.Tests.Tools.SQL;

public abstract class SqlGeneratorBase : ISqlGenerator
{
    #region Fields

    protected StringBuilder Sql = new();

    #endregion

    #region Methods

    public string GenerateSql() => Sql.ToString();

    #endregion
}