namespace Lanceur.Core.Services;

public interface IConfigurationService<out TConfig>
    where TConfig : class, new()
{
    #region Properties

    TConfig Current { get; }

    #endregion

    #region Methods

    void Load();

    void Save();

    #endregion
}