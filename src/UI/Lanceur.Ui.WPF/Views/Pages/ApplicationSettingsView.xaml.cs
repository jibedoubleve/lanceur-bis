using System.IO;
using System.Windows;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Win32;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class ApplicationSettingsView
{
    private readonly IUserNotificationService _userNotification;

    #region Constructors

    public ApplicationSettingsView(
        ApplicationSettingsViewModel viewModel,
        IUserNotificationService userNotification
    )
    {
        _userNotification = userNotification;
        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private ApplicationSettingsViewModel ViewModel { get;  }

    #endregion

    #region Methods

    private void OnClickDbPath(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Filter = "Database|*.db|SQLite database|*.sqlite|All files (*.*)|*.*" };

        if (openFileDialog.ShowDialog() != true) return;

        if (!File.Exists(openFileDialog.FileName))
        {
            _userNotification.Warn("The specified file could not be found.", "File not found");
            return;
        }

        ViewModel.DbPath = openFileDialog.FileName;
    }

    #endregion
}