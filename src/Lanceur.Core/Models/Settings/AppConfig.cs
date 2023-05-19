namespace Lanceur.Core.Models.Settings
{
    public class AppConfig
    {
        #region Properties

        public HotKeySection HotKey { get; set; } = HotKeySection.Default;
        public long IdSession { get; set; }
        public RepositorySection Repository { get; set; } = RepositorySection.Default;
        public double RestartDelay { get; set; } = 500;
        public bool ShowAtStartup { get; set; } = true;
        public WindowSection Window { get; set; } = WindowSection.Default;

        #endregion Properties
    }
}