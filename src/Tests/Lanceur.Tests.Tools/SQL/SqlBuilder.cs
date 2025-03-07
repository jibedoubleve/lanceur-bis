using System.Text;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tools.SQL;

public class SqlBuilder
{
    #region Fields

    private readonly StringBuilder _sql = new();

    #endregion

    #region Constructors

    public SqlBuilder(SqlBuilder? initialSql = null)
    {
        if (initialSql is not null) _sql.Append(initialSql);
    }

    #endregion

    #region Properties

    public static SqlBuilder Empty => new();

    #endregion

    #region Methods

    public SqlBuilder AppendAlias(long idAlias, string? fileName = null, string? arguments = null, Action<AliasSqlBuilder>? aliasSql = null)
    {
        fileName ??= Guid.NewGuid().ToString();
        arguments ??= Guid.NewGuid().ToString();

        _sql.Append($"insert into alias (id, file_name, arguments) values ({idAlias}, '{fileName}', '{arguments}');");
        _sql.AppendNewLine();

        var builder = new AliasSqlBuilder(idAlias, _sql);
        aliasSql?.Invoke(builder);
        return this;
    }

    public override string ToString() => _sql.ToString();

    #endregion
}