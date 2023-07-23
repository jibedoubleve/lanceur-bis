using Lanceur.Infra.Plugins;

namespace Lanceur.Infra.Stores
{
    public interface IPluginStoreContext
    {
        #region Properties

        string RepositoryPath { get; }

        #endregion Properties
    }

    public class PluginStoreContext : IPluginStoreContext
    {
        #region Constructors

        public PluginStoreContext()
        {
            CreateIfNotExist();
        }

        #endregion Constructors

        #region Properties

        public string RepositoryPath => PluginLocation.Root;

        #endregion Properties

        #region Methods

        private void CreateIfNotExist()
        {
            if (!Directory.Exists(RepositoryPath))
            {
                Directory.CreateDirectory(RepositoryPath);
            }
        }

        #endregion Methods
    }
}