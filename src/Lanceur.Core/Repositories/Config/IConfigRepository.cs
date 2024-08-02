namespace Lanceur.Core.Repositories.Config;

public interface IConfigRepository<TConfig>
{
    #region Properties

    TConfig Current { get; }

    #endregion Properties

    #region Methods

    void Load();

    void Save();

    #endregion Methods
}