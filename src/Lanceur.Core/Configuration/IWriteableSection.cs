namespace Lanceur.Core.Configuration;

public interface IWriteableSection<out T> : ISection<T>
{
    #region Methods

    void Reload();
    void Save();

    #endregion
}