using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class VersionView : UserControl
{
    #region Fields

    private const string GithubUrl = "https://github.com/jibedoubleve/lanceur-bis";

    #endregion

    #region Constructors

    public VersionView(string version, string sermver)
    {
        Version = version;
        Commit = sermver;
        InitializeComponent();
    }

    #endregion

    #region Properties

    public string Commit { get; }
    public string Version { get; }

    #endregion

    #region Methods

    private void OnClickWebsite(object sender, RoutedEventArgs e)
    {
        var sInfo = new ProcessStartInfo(GithubUrl) { UseShellExecute = true };
        Process.Start(sInfo);
    }

    #endregion
}