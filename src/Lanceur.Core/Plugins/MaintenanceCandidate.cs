namespace Lanceur.Core.Plugins
{
    public class MaintenanceCandidate
    {
        #region Constructors

        private MaintenanceCandidate(string path, MaintenanceAction maintenanceAction)
        {
            Directory = path;
            MaintenanceAction = maintenanceAction;
        }

        public MaintenanceCandidate()
        {
        }

        #endregion Constructors

        #region Properties

        public string Directory { get; set; }

        public MaintenanceAction MaintenanceAction { get; set; }

        #endregion Properties

        #region Methods

        public static MaintenanceCandidate InstallCandidate(string path) => new(path, MaintenanceAction.Install);

        public static MaintenanceCandidate UninstallCandidate(string path) => new(path, MaintenanceAction.Uninstall);

        #endregion Methods
    }
}