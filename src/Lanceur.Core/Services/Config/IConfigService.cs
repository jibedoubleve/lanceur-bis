namespace Lanceur.Core.Services.Config
{
    public interface IConfigService<TConfig>
    {
        #region Properties

        TConfig Current { get; }

        #endregion Properties

        #region Methods

        void Load();

        void Save();

        #endregion Methods
    }
}