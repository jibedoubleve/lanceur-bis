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
            var doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = @"Lanceur2\";
            RepositoryPath = Path.Combine(doc, dir);

            CreateIfNotExist();
        }

        #endregion Constructors

        #region Properties

        public string RepositoryPath { get; }

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