using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class VersionView : UserControl
{
    #region Fields

    private const string GithubUrl =Paths.GithubUrl;

    #endregion

    #region Constructors

    public VersionView(CurrentVersion currentVersion)
    {
        Version = currentVersion.Version.ToString();
        Commit = currentVersion.Commit;
        Suffix = currentVersion.Suffix;
        InitializeComponent();
    }

    #endregion

    #region Properties

    public string Commit { get; }
    public string Version { get; }
    public string Suffix { get; set; }
    #endregion

    #region Methods

    private void OnClickWebsite(object sender, RoutedEventArgs e)
    {
        var sInfo = new ProcessStartInfo(GithubUrl) { UseShellExecute = true };
        Process.Start(sInfo);
    }

    #endregion
}