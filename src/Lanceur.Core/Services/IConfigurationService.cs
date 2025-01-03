namespace Lanceur.Core.Services;

public interface IConfigurationService<TConfig>
{
    #region Properties

    TConfig Current { get; }

    #endregion Properties

    #region Methods

    void Load();

    void Save();

    #endregion Methods
}