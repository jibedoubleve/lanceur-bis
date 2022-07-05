namespace Lanceur.Core.Managers
{
    public interface IDataStoreUpdateManager
    {
        #region Methods

        Version GetLatestVersion();

        void SetLatestVersion();

        void UpdateFrom(string version);

        void UpdateFrom(Version version);

        void UpdateFromScractch();

        #endregion Methods
    }
}