namespace Lanceur.Core.Models.Settings
{
    public class AppSettings
    {
        #region Properties

        public HotKeySection HotKey { get; set; } = HotKeySection.Empty;
        public long IdSession { get; set; }
        public RepositorySection Repository { get; set; } = new RepositorySection();
        public bool ShowAtStartup { get; set; }
        public WindowSection Window { get; set; } = new WindowSection();

        #endregion Properties
    }
}