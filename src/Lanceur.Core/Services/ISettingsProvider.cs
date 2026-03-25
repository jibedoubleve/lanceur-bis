namespace Lanceur.Core.Services;

/// <summary>
///     Non-generic base contract for settings providers.
///     Exposes <see cref="Value" /> as <see cref="object" /> to allow uniform enumeration
///     (e.g. in <c>Section&lt;T&gt;</c>) without knowing the concrete configuration type.
/// </summary>
public interface ISettingsProvider
{
    #region Properties

    /// <summary>
    ///     Gets the current configuration instance as an untyped object.
    /// </summary>
    /// <value>
    ///     The current configuration object. Cast to the expected type or use
    ///     <see cref="ISettingsProvider{TConfig}.Value" /> for a typed alternative.
    /// </value>
    object Value { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Loads the settings from the underlying storage mechanism.
    /// </summary>
    /// <remarks>
    ///     This method reads the configuration from its source (such as disk, memory, or another persistence layer)
    ///     and updates the <see cref="Value" /> property.
    /// </remarks>
    void Load();

    /// <summary>
    ///     Saves the current settings to the underlying storage mechanism.
    /// </summary>
    /// <remarks>
    ///     This method persists the current state of the configuration to its destination (such as disk, memory, or another
    ///     persistence layer).
    /// </remarks>
    void Save();

    #endregion
}

/// <summary>
///     Defines a contract for managing application settings of type <typeparamref name="TConfig" />.
/// </summary>
/// <typeparam name="TConfig">The type of configuration object. Must be a reference type with a parameterless constructor.</typeparam>
public interface ISettingsProvider<out TConfig> : ISettingsProvider
    where TConfig : class, new()
{
    #region Properties

    /// <summary>
    ///     Gets the current configuration instance as <typeparamref name="TConfig" />.
    ///     Hides <see cref="ISettingsProvider.Value" /> to provide a strongly-typed alternative.
    /// </summary>
    /// <value>
    ///     The current configuration object of type <typeparamref name="TConfig" />.
    /// </value>
    new TConfig Value { get; }

    #endregion
}