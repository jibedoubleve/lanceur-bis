namespace Lanceur.Core.Services
{
    public enum Setting
    {
        DbPath,
    }

    public interface ISettingsService
    {
        #region Indexers

        string this[Setting key] { get; set; }

        #endregion Indexers

        #region Methods

        void Load();

        void Save();

        #endregion Methods
    }
}