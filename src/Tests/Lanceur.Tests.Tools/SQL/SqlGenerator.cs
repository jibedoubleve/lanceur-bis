namespace Lanceur.Tests.Tools.SQL;

public class SqlGenerator : SqlGeneratorBase
{
    #region Properties

    /// <summary>
    /// Tracks the last identifier assigned to an alias.
    /// A value of 0 means that no alias has been created yet.
    /// </summary>
    public int IdSequence  { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a new alias and applies the given configuration action to it.
    /// The alias is assigned a unique identifier from an internal counter.
    /// </summary>
    /// <param name="generator">
    /// A delegate used to configure the newly created <see cref="SqlAliasGenerator"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="SqlGenerator"/> instance, allowing method chaining.
    /// </returns>
    public SqlGenerator AppendAlias(Action<SqlAliasGenerator> generator)
    {
        generator(new(++IdSequence, Sql));
        return this;
    }

    #endregion
}