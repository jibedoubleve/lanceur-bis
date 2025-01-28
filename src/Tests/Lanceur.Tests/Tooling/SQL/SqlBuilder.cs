using System.Text;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tooling.SQL;

public class SqlBuilder
{
    #region Fields

    private readonly StringBuilder _sql = new();

    #endregion

    #region Constructors

    public SqlBuilder(SqlBuilder initialSql = null)
    {
        if (initialSql is not null) _sql.Append(initialSql);
    }

    #endregion

    #region Properties

    public static SqlBuilder Empty => new();

    #endregion

    #region Methods

    public SqlBuilder AppendAlias(long idAlias, string fileName = null, string arguments = null, string[] synonyms = null)
    {
        fileName ??= Guid.NewGuid().ToString();
        arguments ??= Guid.NewGuid().ToString();

        _sql.Append($"insert into alias (id, file_name, arguments) values ({idAlias}, '{fileName}', '{arguments}');");
        _sql.AppendNewLine();

        if (synonyms is null) return this;

        AppendSynonyms(idAlias, synonyms);
        return this;
    }

    public SqlBuilder AppendArgument(long idAlias, string name = null, string argument = null)
    {
        name ??= Guid.NewGuid().ToString();
        argument ??= Guid.NewGuid().ToString();

        _sql.Append($"insert into alias_argument(id_alias, name, argument) values ({idAlias}, '{name}', '{argument}');");
        _sql.AppendNewLine();
        return this;
    }

    public SqlBuilder AppendSynonyms(long idAlias, params string[] synonyms)
    {
        if (synonyms is null || synonyms.Length == 0) throw new ArgumentNullException(nameof(synonyms), "You should provide names for the alias");

        foreach (var synonym in synonyms)
        {
            _sql.Append($"insert into alias_name(id_alias, name) values ({idAlias}, '{synonym}');");
            _sql.AppendNewLine();
        }

        return this;
    }

    public override string ToString() => _sql.ToString();

    #endregion
}