namespace Lanceur.Core.Configuration.Sections;

public interface IWriteableSection<out T> : ISection<T>
{
    #region Methods

    void Reload();
    void Save();

    #endregion
}