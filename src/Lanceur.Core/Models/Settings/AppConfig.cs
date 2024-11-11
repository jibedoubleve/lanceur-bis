namespace Lanceur.Core.Models.Settings;

public class AppConfig
{
    #region Properties

    public HotKeySection HotKey { get; set; } = HotKeySection.Default;
    public RepositorySection Repository { get; set; } = RepositorySection.Default;
    public double SearchDelay { get; set; } = 50;
    public WindowSection Window { get; set; } = WindowSection.Default;

    #endregion Properties
}