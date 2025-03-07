using System.Globalization;
using System.Text;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tools.SQL;

public class AliasSqlBuilder
{
    #region Fields

    private readonly long _idAlias;

    private readonly StringBuilder _sql;

    #endregion

    #region Constructors

    public AliasSqlBuilder(long idAlias, StringBuilder sql)
    {
        _idAlias = idAlias;
        _sql = sql;
    }

    #endregion

    #region Methods

    public AliasSqlBuilder WithArgument(string? name = null, string? argument = null)
    {
        name ??= Guid.NewGuid().ToString();
        argument ??= Guid.NewGuid().ToString();

        _sql.Append($"insert into alias_argument(id_alias, name, argument) values ({_idAlias}, '{name}', '{argument}');");
        _sql.AppendNewLine();
        return this;
    }

    public AliasSqlBuilder WithSynonyms(params string[] synonyms)
    {
        if (synonyms is null || synonyms.Length == 0) throw new ArgumentNullException(nameof(synonyms), "You should provide names for the alias");

        foreach (var synonym in synonyms)
        {
            _sql.Append($"insert into alias_name(id_alias, name) values ({_idAlias}, '{synonym}');");
            _sql.AppendNewLine();
        }

        return this;
    }

    public AliasSqlBuilder WithUsage(params DateTime[] dates)
    {
        foreach (var date in dates)
        {
            var dateStr = date.ToString("o", CultureInfo.InvariantCulture);
            _sql.Append($"insert into alias_usage (id_alias, time_stamp) values ({_idAlias}, '{dateStr}');");
        }

        return this;
    }

    #endregion
}