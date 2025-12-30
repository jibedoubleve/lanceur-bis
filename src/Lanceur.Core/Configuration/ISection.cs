namespace Lanceur.Core.Configuration;

public interface ISection<out T>
{
    #region Properties

    T Value { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reloads the configuration section from its source, updating all values.
    /// </summary>
    void Reload();

    #endregion
}