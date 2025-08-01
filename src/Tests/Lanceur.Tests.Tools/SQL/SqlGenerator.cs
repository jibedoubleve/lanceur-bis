namespace Lanceur.Tests.Tools.SQL;

public class SqlGenerator : SqlGeneratorBase
{
    #region Methods

    public SqlGenerator AppendAlias(long idAlias, Action<SqlAliasGenerator> generator)
    {
        generator(new(idAlias, Sql));
        return this;
    }

    #endregion
}