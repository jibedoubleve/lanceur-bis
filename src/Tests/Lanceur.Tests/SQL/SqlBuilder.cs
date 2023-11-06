using Lanceur.SharedKernel.Mixins;
using System.Text;

namespace Lanceur.Tests.SQL;

internal class SqlBuilder
{
    #region Fields

    private readonly StringBuilder _sql = new();
    private long _sequence;

    #endregion Fields

    #region Constructors

    public SqlBuilder(SqlBuilder initialSql = null)
    {
        if (initialSql is not null) _sql.Append(initialSql);
    }

    #endregion Constructors

    #region Methods

    public SqlBuilder AppendAlias(string fileName, string arguments, params string[] synonyms)
        => AppendAlias(++_sequence, fileName, arguments, synonyms);

    public SqlBuilder AppendAlias(long id, string fileName, string arguments, params string[] synonyms)
    {
        if (id < _sequence) throw new InvalidDataException($"The id '{id}' is below the minimum sequence value ({_sequence})");

        _sequence = id;
        _sql.Append($"insert into alias (id, file_name, arguments, id_session) values ({id}, '{fileName}', '{arguments}', 1);");
        _sql.AppendNewLine();
        foreach (var synonym in synonyms)
        {
            _sql.Append($"insert into alias_name(id, id_alias, name) values ({++_sequence}, {id}, '{synonym}');");
            _sql.AppendNewLine();
        }
        return this;
    }

    public override string ToString() => _sql.ToString();

    #endregion Methods
}