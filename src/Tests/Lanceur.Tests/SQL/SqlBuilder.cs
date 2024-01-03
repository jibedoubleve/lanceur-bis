using Lanceur.SharedKernel.Mixins;
using System.Text;

namespace Lanceur.Tests.SQL;

internal class SqlBuilder
{
    #region Fields

    private readonly StringBuilder _sql = new();

    #endregion Fields

    #region Constructors

    public SqlBuilder(SqlBuilder initialSql = null)
    {
        if (initialSql is not null) _sql.Append(initialSql);
    }

    #endregion Constructors

    #region Methods

    public SqlBuilder AppendAlias(long idAlias, string fileName, string arguments)
    {
        _sql.Append($"insert into alias (id, file_name, arguments, id_session) values ({idAlias}, '{fileName}', '{arguments}', 1);");
        _sql.AppendNewLine();
        return this;
    }

    public SqlBuilder AppendArgument(long idAlias, string name, string argument)
    {
        _sql.Append($"insert into alias_argument(id_alias, name, argument) values ({idAlias}, '{name}', '{argument}');");
        _sql.AppendNewLine();
        return this;
    }

    public SqlBuilder AppendSynonyms(long idAlias, params string[] synonyms)
    {
        if (synonyms is null || synonyms.Length == 0) { throw new ArgumentNullException(nameof(synonyms), "You should provide names for the alias"); }

        foreach (var synonym in synonyms)
        {
            _sql.Append($"insert into alias_name(id_alias, name) values ({idAlias}, '{synonym}');");
            _sql.AppendNewLine();
        }
        return this;
    }

    public override string ToString() => _sql.ToString();

    #endregion Methods
}