namespace Lanceur.Tests.Tools.SQL;

public  class Sql : ISqlGenerator
{
    #region Methods

    public static ISqlGenerator Empty => new Sql();
    public string Generate() => "";

    #endregion
}