namespace Lanceur.Core.Configuration.Sections;

/// <summary>
///     Represents a configuration section that can be persisted to its underlying storage.
/// </summary>
/// <typeparam name="T">The type of configuration data contained in the section.</typeparam>
public interface IWriteableSection<out T> : ISection<T>
{
    #region Methods

    /// <summary>
    ///     Persists the current configuration section values to the underlying storage.
    /// </summary>
    void Save();

    #endregion
}