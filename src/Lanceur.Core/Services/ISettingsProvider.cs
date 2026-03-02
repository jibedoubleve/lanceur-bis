namespace Lanceur.Core.Services;

/// <summary>
///     Defines a contract for managing application settings of type <typeparamref name="TConfig" />.
/// </summary>
/// <typeparam name="TConfig">The type of configuration object. Must be a reference type with a parameterless constructor.</typeparam>
public interface ISettingsProvider<out TConfig>
    where TConfig : class, new()
{
    #region Properties

    /// <summary>
    ///     Gets the current configuration instance.
    /// </summary>
    /// <value>
    ///     The current configuration object of type <typeparamref name="TConfig" />.
    /// </value>
    TConfig Current { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Loads the settings from the underlying storage mechanism.
    /// </summary>
    /// <remarks>
    ///     This method reads the configuration from its source (such as disk, memory, or another persistence layer)
    ///     and updates the <see cref="Current" /> property.
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