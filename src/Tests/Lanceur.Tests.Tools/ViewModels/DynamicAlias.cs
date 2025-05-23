namespace Lanceur.Tests.Tools.ViewModels;

/// <summary>
///     Represents a dynamic alias used in SQL queries to retrieve a single field.
///     The SQL query determines which field is mapped to the <see cref="FieldValue" /> property.
/// </summary>
/// <typeparam name="T">The type of the dynamically mapped field.</typeparam>
public record DynamicAlias<T>
{
    #region Properties

    /// <summary>
    ///     A generic field that stores a dynamically mapped value from the SQL query.
    ///     The SQL query defines which field is assigned to this property.
    /// </summary>
    public required T FieldValue { get; init; }

    /// <summary>
    ///     The unique identifier of the alias.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     The name of the alias.
    /// </summary>
    public required string Name { get; init; }

    #endregion
}