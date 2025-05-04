using System.Text;
using Lanceur.SharedKernel;
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

    /// <summary>
    ///     Appends a new alias to the SQL script.
    /// </summary>
    /// <param name="idAlias">The primary key of the alias.</param>
    /// <param name="fileName">
    ///     The file name for the alias. If <c>null</c>, a random value will be assigned.
    ///     If the string <c>"null"</c> is provided, a null value will be set in the database.
    /// </param>
    /// <param name="arguments">
    ///     The arguments for the alias. If <c>null</c>, a random value will be assigned.
    ///     If the string <c>"null"</c> is provided, a null value will be set in the database.
    /// </param>
    /// <param name="props">Additional properties to apply to the alias</param>
    /// <param name="cfg">
    ///     A configurator that allows customisation of the alias, including Usage, Additional Parameters, or Synonyms.
    ///     If the configurator is omitted, a default name will be provided to the alias based on this pattern: "alias_{idAlias}".
    /// </param>
    /// <returns>The updated <see cref="SqlBuilder" /> instance.</returns>
    public SqlBuilder AppendAlias(long idAlias, string? fileName = null, string? arguments = null, AliasProps? props = null, Action<AliasSqlBuilder>? cfg = null)
    {
        _sql.Append(AliasSqlBuilder.GenerateAliasSql(idAlias, fileName, arguments, props));
        _sql.AppendNewLine();

        var builder = new AliasSqlBuilder(idAlias, _sql);
        
        if (cfg is not null) { cfg.Invoke(builder); }
        else { builder.WithSynonyms($"alias_{idAlias}"); }
        return this;
    }

    public override string ToString() => _sql.ToString();

    #endregion
}

public record AliasProps(Constants.RunAs? RunAs = null, Constants.StartMode? StartMode = null, DateTime? DeletedAt = null);