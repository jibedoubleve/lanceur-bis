namespace Lanceur.Tests.Tools.SQL;

public  class Sql : ISqlGenerator
{
    #region Constructors

    /// <summary>
    ///     Avoid call of the constructor. Use 'Empty instead"
    /// </summary>
    private Sql() { }

    #endregion

    #region Properties

    public static ISqlGenerator Empty => new Sql();

    #endregion

    #region Methods

    public string GenerateSql() => "";

    #endregion
}