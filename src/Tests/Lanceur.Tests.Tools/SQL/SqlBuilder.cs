namespace Lanceur.Tests.Tools.SQL;

public class SqlBuilder : SqlBuilderBase
{
    #region Properties

    /// <summary>
    ///     Represents an empty generator, that's no sql will be generated from this instance
    /// </summary>
    public static SqlBuilder Empty { get; } = new();

    /// <summary>
    ///     Tracks the last identifier assigned to an alias.
    ///     A value of 0 means that no alias has been created yet.
    /// </summary>
    public int IdSequence  { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Creates a new alias and applies the given configuration action to it.
    ///     The alias is assigned a unique identifier from an internal counter.
    /// </summary>
    /// <param name="generator">
    ///     A delegate used to configure the newly created <see cref="SqlAliasBuilder" />.
    /// </param>
    /// <returns>
    ///     The current <see cref="SqlBuilder" /> instance, allowing method chaining.
    /// </returns>
    public SqlBuilder AppendAlias(Action<SqlAliasBuilder> generator)
    {
        generator(new(++IdSequence, Sql));
        return this;
    }

    #endregion
}