namespace Lanceur.Tests.Tools.SQL;

public  class Sql : ISqlBuilder
{
    #region Constructors

    /// <summary>
    ///     Avoid call of the constructor. Use 'Empty instead"
    /// </summary>
    private Sql() { }

    #endregion

    #region Properties

    public static ISqlBuilder Empty => new Sql();

    #endregion

    #region Methods

    public string ToSql() => "";

    #endregion
}