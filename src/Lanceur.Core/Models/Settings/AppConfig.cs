namespace Lanceur.Core.Models.Settings
{
    public class AppConfig
    {
        #region Properties

        public HotKeySection HotKey { get; set; } = HotKeySection.Default;
        public long IdSession { get; set; }
        public RepositorySection Repository { get; set; } = RepositorySection.Default;
        public double RestartDelay { get; set; } = 500;
        public WindowSection Window { get; set; } = WindowSection.Default;
        public bool EverythingResultsShowIcon { get; set; } = false;

        #endregion Properties
    }
}